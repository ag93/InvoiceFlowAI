import { useState, type SubmitEvent } from 'react'
import {
  approveDocument,
  ApprovalApiError
} from '../api/documents'
import type { InvoiceExtraction } from '../types/analysis'
import type {
  ApprovedInvoice,
  ApprovalValidationIssue
} from '../types/approval'

interface InvoiceReviewFormProps {
  documentId: string
  extraction: InvoiceExtraction
  onApproved: (invoice: ApprovedInvoice) => void
}

export function InvoiceReviewForm({
  documentId,
  extraction,
  onApproved
}: InvoiceReviewFormProps) {
  const [vendor, setVendor] = useState(extraction.vendor ?? '')
  const [invoiceNumber, setInvoiceNumber] =
    useState(extraction.invoiceNumber ?? '')
  const [invoiceDate, setInvoiceDate] =
    useState(extraction.date ?? '')
  const [total, setTotal] = useState(
    extraction.total === null ? '' : String(extraction.total)
  )
  const [currency, setCurrency] =
    useState(extraction.currency ?? '')

  const [submitting, setSubmitting] = useState(false)
  const [approved, setApproved] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [issues, setIssues] =
    useState<ApprovalValidationIssue[]>([])

  async function handleApprove(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()

    if (submitting || approved) {
      return
    }

    setError(null)
    setIssues([])
    setSubmitting(true)

    try {
      const parsedTotal =
        total.trim() === '' ? null : Number(total)

      if (
        parsedTotal === null ||
        !Number.isFinite(parsedTotal) ||
        parsedTotal <= 0 ||
        !/^\d+(\.\d{1,2})?$/.test(total.trim())
      ) {
        setIssues([{
          field: 'Total',
          message: 'Enter a positive amount with up to two decimal places.'
        }])
        return
      }

      const invoice = await approveDocument(documentId, {
        vendor,
        invoiceNumber,
        invoiceDate,
        total: parsedTotal,
        currency,
      })

      setApproved(true)
      onApproved(invoice)
    } catch (err) {
      if (err instanceof ApprovalApiError) {
        setIssues(err.issues)
      }

      setError(
        err instanceof Error
          ? err.message
          : 'An unexpected approval error occurred.'
      )
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="review-form" onSubmit={handleApprove}>
      <div className="review-heading">
        <h4>Review and approve</h4>
        <p>
          Verify these values against the original PDF.
          Correct any mistakes before approving.
        </p>
      </div>

      <div className="review-grid">
        <label>
          Vendor
          <input
            type="text"
            value={vendor}
            onChange={event => setVendor(event.target.value)}
            disabled={submitting || approved}
          />
        </label>

        <label>
          Invoice number
          <input
            type="text"
            value={invoiceNumber}
            onChange={event => setInvoiceNumber(event.target.value)}
            disabled={submitting || approved}
          />
        </label>

        <label>
          Invoice date
          <input
            type="date"
            value={invoiceDate}
            onChange={event => setInvoiceDate(event.target.value)}
            disabled={submitting || approved}
          />
        </label>

        <label>
          Total
          <input
            type="text"
            inputMode="decimal"
            value={total}
            onChange={event => setTotal(event.target.value)}
            disabled={submitting || approved}
          />
        </label>

        <label>
          Currency
          <input
            type="text"
            maxLength={3}
            value={currency}
            onChange={event =>
              setCurrency(event.target.value.toUpperCase())
            }
            disabled={submitting || approved}
          />
        </label>
      </div>

      {issues.length > 0 && (
        <ul className="validation-issues" role="alert">
          {issues.map((issue, index) => (
            <li key={`${issue.field}-${index}`}>
              <strong>{issue.field}:</strong> {issue.message}
            </li>
          ))}
        </ul>
      )}

      {error && <p className="upload-error" role="alert">{error}</p>}

      {approved ? (
        <p className="validation-success" role="status">
          Invoice approved and saved successfully!
        </p>
      ) : (
        <button
          type="submit"
          className="upload-button"
          disabled={submitting}
        >
          {submitting ? 'Approving...' : 'Approve invoice'}
        </button>
      )}
    </form>
  )
}