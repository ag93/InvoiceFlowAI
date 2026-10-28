import { useState } from 'react'
import { analyzeDocument } from '../api/documents'
import type {
  InvoiceProcessingResult
} from '../types/analysis'
import { InvoiceReviewForm } from './InvoiceReviewForm'
import type { Invoice } from '../types/invoice'

interface DocumentAnalysisProps {
  documentId: string
  onApproved: (invoice: Invoice) => void
}

export function DocumentAnalysis({
  documentId,
  onApproved
}: DocumentAnalysisProps) {
  const [result, setResult] =
    useState<InvoiceProcessingResult | null>(null)

  const [analyzing, setAnalyzing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleAnalyze() {
    if (analyzing || result !== null) {
      return
    }

    setAnalyzing(true)
    setError(null)

    try {
      const response = await analyzeDocument(documentId)
      setResult(response)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'An unexpected analysis error occurred.'
      )
    } finally {
      setAnalyzing(false)
    }
  }

  return (
    <section className="analysis-section">
      <div className="analysis-header">
        <h4>Invoice analysis</h4>
        <p>
          Extract invoice fields using Azure AI Content Understanding.
        </p>
      </div>

      {!result && (
        <button
          type="button"
          className="upload-button"
          onClick={() => void handleAnalyze()}
          disabled={analyzing}
        >
          {analyzing ? 'Analyzing invoice...' : 'Analyze invoice'}
        </button>
      )}

      {analyzing && (
        <p className="analysis-notice" role="status">
          Azure AI is reading your document. This may take a moment.
        </p>
      )}

      {error && (
        <p className="upload-error" role="alert">
          {error}
        </p>
      )}

      {result && (
        <div className="analysis-results">
          <div className="result-heading">
            <h4>Extracted information</h4>
            <span className="status-approved">
              Analysis complete
            </span>
          </div>

          <dl className="extracted-fields">
            <div>
              <dt>Vendor</dt>
              <dd>{result.extraction.vendor ?? 'Not found'}</dd>
            </div>

            <div>
              <dt>Invoice number</dt>
              <dd>
                {result.extraction.invoiceNumber ?? 'Not found'}
              </dd>
            </div>

            <div>
              <dt>Invoice date</dt>
              <dd>{result.extraction.date ?? 'Not found'}</dd>
            </div>

            <div>
              <dt>Total</dt>
              <dd>
                {result.extraction.total === null
                  ? 'Not found'
                  : `${result.extraction.total.toFixed(2)} ${
                      result.extraction.currency ?? ''
                    }`}
              </dd>
            </div>

            <div>
              <dt>Currency</dt>
              <dd>{result.extraction.currency ?? 'Not found'}</dd>
            </div>
          </dl>

          <div className="validation-panel">
            <h4>Validation results</h4>

            {result.validation.isValid ? (
              <p className="validation-success">
                All business validation rules passed.
                Human review is still required before approval.
              </p>
            ) : (
              <>
                <p className="upload-error">
                  Review the following issues:
                </p>

                <ul className="validation-issues">
                  {result.validation.issues.map((issue, index) => (
                    <li key={`${issue.field}-${index}`}>
                      <strong>{issue.field}:</strong> {issue.message}
                    </li>
                  ))}
                </ul>
              </>
            )}
          </div>
          <InvoiceReviewForm
            documentId={documentId}
            extraction={result.extraction}
            onApproved={onApproved}
            />
        </div>
      )}
    </section>
  )
}