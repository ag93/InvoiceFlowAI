export interface UploadDocumentResponse {
  documentId: string
}

import type {
  InvoiceProcessingResult
} from '../types/analysis'

import type {
  ApproveInvoiceRequest,
  ApprovedInvoice,
  ApprovalValidationResponse
} from '../types/approval'

export async function uploadDocument(
  file: File
): Promise<UploadDocumentResponse> {
  const formData = new FormData()

  formData.append('file', file)

  const response = await fetch('/api/documents', {
    method: 'POST',
    body: formData,
  })

  if (!response.ok) {
    throw new Error(
      `Upload failed (HTTP ${response.status})`
    )
  }

  const data: unknown = await response.json()

  if (
    typeof data !== 'object' ||
    data === null ||
    !('documentId' in data) ||
    typeof data.documentId !== 'string'
  ) {
    throw new Error('Unexpected upload response from API')
  }

  return { documentId: data.documentId }
}

export async function analyzeDocument(
  documentId: string
): Promise<InvoiceProcessingResult> {
  const response = await fetch(
    `/api/documents/${encodeURIComponent(documentId)}/analyze`,
    { method: 'POST' }
  )

  if (!response.ok) {
    if (response.status === 404) {
      throw new Error('The document could not be found.')
    }

    if (response.status === 409) {
      throw new Error(
        'This document is already processing or has been approved.'
      )
    }

    throw new Error(
      `Invoice analysis failed (HTTP ${response.status}).`
    )
  }

  const result: InvoiceProcessingResult = await response.json()

  return result
}

export class ApprovalApiError extends Error {
  readonly status: number
  readonly issues: ApprovalValidationResponse['issues']

  constructor(
    message: string,
    status: number,
    issues: ApprovalValidationResponse['issues'] = []
  ) {
    super(message)

    this.name = 'ApprovalApiError'
    this.status = status
    this.issues = issues
  }
}

export async function approveDocument(
  documentId: string,
  request: ApproveInvoiceRequest
): Promise<ApprovedInvoice> {
  const response = await fetch(
    `/api/documents/${encodeURIComponent(documentId)}/approve`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    }
  )

  if (!response.ok) {
    if (response.status === 400) {
      const validation: ApprovalValidationResponse =
        await response.json()

      throw new ApprovalApiError(
        'Please correct the highlighted validation issues.',
        400,
        validation.issues ?? []
      )
    }

    if (response.status === 404) {
      throw new ApprovalApiError('Document not found.', 404)
    }

    if (response.status === 409) {
      throw new ApprovalApiError(
        'This document cannot be approved in its current state.',
        409
      )
    }

    throw new ApprovalApiError(
      `Approval failed (HTTP ${response.status}).`,
      response.status
    )
  }

  return (await response.json()) as ApprovedInvoice
}
