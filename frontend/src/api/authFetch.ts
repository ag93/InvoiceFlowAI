import {
  InteractionRequiredAuthError,
} from '@azure/msal-browser'

import {
  invoiceApiScopes,
  msalInstance,
} from '../auth/msalConfig'

export async function authFetch(
  input: string,
  init?: RequestInit
): Promise<Response> {
  const account =
    msalInstance.getActiveAccount() ??
    msalInstance.getAllAccounts()[0]

  if (!account) {
    throw new Error(
      'Please sign in before accessing invoices.'
    )
  }

  let accessToken: string

  try {
    const tokenResult =
      await msalInstance.acquireTokenSilent({
        account,
        scopes: invoiceApiScopes,
      })

    accessToken = tokenResult.accessToken
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      throw new Error(
        'Microsoft Entra needs you to sign in or grant permission again.'
      )
    }

    throw error
  }

  const headers = new Headers(init?.headers)

  headers.set(
    'Authorization',
    `Bearer ${accessToken}`
  )

  return fetch(input, {
    ...init,
    headers,
  })
}