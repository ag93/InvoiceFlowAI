import type { Invoice } from './invoice'

export interface ApproveInvoiceRequest {
  vendor: string
  invoiceNumber: string
  invoiceDate: string
  total: number
  currency: string
}

export type ApprovedInvoice = Invoice

export interface ApprovalValidationIssue {
  field: string
  message: string
}

export interface ApprovalValidationResponse {
  isValid: boolean
  issues: ApprovalValidationIssue[]
}