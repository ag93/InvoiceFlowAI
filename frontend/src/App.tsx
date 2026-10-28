import { useEffect, useState } from 'react'
import './App.css'

function App() {
  const [message, setMessage] = useState('Connecting to API...')

  useEffect(() => {
    async function checkApi() {
      try {
        const response = await fetch('/api/invoices')

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`)
        }

        const invoices: unknown = await response.json()

        if (!Array.isArray(invoices)) {
          throw new Error('Unexpected API response')
        }

        setMessage(
          `Connected! ${invoices.length} approved invoice(s) found.`
        )
      } catch (error) {
        setMessage(
          error instanceof Error
            ? `API connection failed: ${error.message}`
            : 'API connection failed.'
        )
      }
    }

    void checkApi()
  }, [])

  return (
    <main>
      <h1>InvoiceFlow AI</h1>
      <p>React + TypeScript frontend</p>
      <p>{message}</p>
    </main>
  )
}

export default App