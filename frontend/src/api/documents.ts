export interface UploadDocumentResponse {
  documentId: string
}

import type {
  InvoiceProcessingResult
} from '../types/analysis'

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