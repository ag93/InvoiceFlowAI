import type { Invoice } from '../types/invoice'
import { authFetch } from './authFetch'

export async function getInvoices(signal?: AbortSignal): Promise<Invoice[]> {
  const response = await authFetch('/api/invoices', { signal })

  if (!response.ok) {
    throw new Error(`Could not load invoices (HTTP ${response.status})`)
  }

  const data: unknown = await response.json()

  if (!Array.isArray(data)) {
    throw new Error('Unexpected invoice response from the API')
  }

  return data as Invoice[]
}