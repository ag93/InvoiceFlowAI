import { useEffect, useState } from 'react'
import { getInvoices } from './api/invoices'
import type { Invoice } from './types/invoice'
import { DocumentUpload } from './components/DocumentUpload'
import './App.css'

function App() {
  const [invoices, setInvoices] = useState<Invoice[]>([])
  const [refreshKey, setRefreshKey] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadInvoices() {
      try {
        setLoading(true)
        setError(null)
        const data = await getInvoices(controller.signal)

        if (!controller.signal.aborted) {
          setInvoices(data)
        }
      } catch (err) {
        if (!controller.signal.aborted) {
          setError(
            err instanceof Error
              ? err.message
              : 'An unexpected error occurred.'
          )
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadInvoices()

    return () => controller.abort()
  }, [refreshKey])

  const vendorCount = new Set(invoices.map(invoice => invoice.vendor)).size

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="brand">
          <div className="brand-mark">IF</div>
          <div>
            <h1>InvoiceFlow AI</h1>
            <p>Invoice management dashboard</p>
          </div>
        </div>
        <span className="environment-label">Local MVP</span>
      </header>

      <main className="main-content">
        <section className="page-heading">
          <div>
            <h2>Overview</h2>
            <p>Manage your approved invoices in one place.</p>
          </div>
        </section>
        <DocumentUpload
        onApproved={() => {
          setRefreshKey(current => current + 1)
        }}/>
          <section className="stats-grid" aria-label="Dashboard statistics">
            <div className="stat-card">
              <span>Approved invoices</span>
              <strong>{loading ? '—' : invoices.length}</strong>
            </div>

            <div className="stat-card">
              <span>Vendors</span>
              <strong>{loading ? '—' : vendorCount}</strong>
            </div>

            <div className="stat-card">
              <span>Review queue</span>
              <strong>—</strong>
              <small>Available in a future step</small>
            </div>
          </section>

          <section className="invoice-panel">
            <div className="panel-header">
              <div>
                <h3>Approved invoices</h3>
                <p>Records stored in SQLite</p>
              </div>
            </div>

            {loading ? (
              <p className="status-message" role="status">
                Loading invoices...
              </p>
            ) : error ? (
              <p className="error-message" role="alert">
                {error}
              </p>
            ) : invoices.length === 0 ? (
              <div className="empty-state">
                <h4>No approved invoices yet</h4>
                <p>
                  Upload and approve an invoice to see it here.
                </p>
              </div>
            ) : (
              <div className="table-scroll">
                <table className="invoice-table">
                  <thead>
                    <tr>
                      <th>Invoice number</th>
                      <th>Vendor</th>
                      <th>Date</th>
                      <th>Amount</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {invoices.map(invoice => (
                      <tr key={invoice.id}>
                        <td className="invoice-number">
                          {invoice.invoiceNumber}
                        </td>
                        <td>{invoice.vendor}</td>
                        <td>{invoice.invoiceDate}</td>
                        <td>
                          {invoice.total.toFixed(2)} {invoice.currency}
                        </td>
                        <td>
                          <span className="status-approved">
                            Approved
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </main>
    </div>
  )
}

export default App