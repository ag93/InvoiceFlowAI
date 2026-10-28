import {
  useState,
  type ChangeEvent,
  type SubmitEvent
} from 'react'
import { uploadDocument } from '../api/documents'
import { DocumentAnalysis } from './DocumentAnalysis'

const MAX_FILE_SIZE = 10 * 1024 * 1024

export function DocumentUpload() {
  const [file, setFile] = useState<File | null>(null)
  const [uploading, setUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [documentId, setDocumentId] = useState<string | null>(null)

  function handleFileChange(
    event: ChangeEvent<HTMLInputElement>
  ) {
    const selectedFile = event.target.files?.[0] ?? null

    setError(null)
    setDocumentId(null)
    setFile(null)

    if (!selectedFile) {
      return
    }

    if (!selectedFile.name.toLowerCase().endsWith('.pdf')) {
      setError('Please select a PDF file.')
      return
    }

    if (selectedFile.size === 0) {
      setError('The selected file is empty.')
      return
    }

    if (selectedFile.size > MAX_FILE_SIZE) {
      setError('The PDF must be 10 MB or smaller.')
      return
    }

    setFile(selectedFile)
  }

  async function handleUpload(
    event: SubmitEvent<HTMLFormElement>
  ) {
    event.preventDefault()

    if (!file || uploading) {
      return
    }

    setUploading(true)
    setError(null)
    setDocumentId(null)

    try {
      const result = await uploadDocument(file)
      setDocumentId(result.documentId)
      setFile(null)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'An unexpected upload error occurred.'
      )
    } finally {
      setUploading(false)
    }
  }

  return (
    <section className="upload-panel">
      <div className="panel-header">
        <h3>Upload an invoice</h3>
        <p>Select a PDF to store it in InvoiceFlow AI.</p>
      </div>

      <form className="upload-form" onSubmit={handleUpload}>
        <label htmlFor="invoice-file">
          Invoice PDF
        </label>

        <input
          id="invoice-file"
          type="file"
          accept=".pdf,application/pdf"
          onChange={handleFileChange}
          disabled={uploading}
        />

        <p className="upload-help">
          PDF only · Maximum file size: 10 MB
        </p>

        {file && (
          <p className="selected-file">
            Selected: {file.name}
          </p>
        )}

        <button
          className="upload-button"
          type="submit"
          disabled={!file || uploading}
        >
          {uploading ? 'Uploading...' : 'Upload PDF'}
        </button>

        {error && (
          <p className="upload-error" role="alert">
            {error}
          </p>
        )}

        {documentId && (
        <div className="uploaded-document">
          <div className="upload-success" role="status">
            <strong>Document uploaded successfully!</strong>
            <p>Document ID:</p>
            <code>{documentId}</code>
          </div>

          <DocumentAnalysis
            key={documentId}
            documentId={documentId}
          />
        </div>
        )}
      </form>
    </section>
  )
}