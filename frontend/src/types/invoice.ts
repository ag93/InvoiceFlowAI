export interface Invoice {
  id: string
  documentId: string
  vendor: string
  invoiceNumber: string
  invoiceDate: string
  total: number
  currency: string
  approvedAt: string
}