export interface InvoiceExtraction {
  vendor: string | null
  invoiceNumber: string | null
  date: string | null
  total: number | null
  currency: string | null
}

export interface ValidationIssue {
  field: string
  message: string
}

export interface InvoiceValidation {
  isValid: boolean
  issues: ValidationIssue[]
}

export interface InvoiceProcessingResult {
  documentId: string
  extraction: InvoiceExtraction
  validation: InvoiceValidation
}