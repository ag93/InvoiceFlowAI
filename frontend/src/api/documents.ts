export interface UploadDocumentResponse {
  documentId: string
}

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