import type { Configuration } from '@azure/msal-browser'
import { PublicClientApplication } from '@azure/msal-browser'

const clientId = import.meta.env.VITE_ENTRA_CLIENT_ID
const tenantId = import.meta.env.VITE_ENTRA_TENANT_ID

if (!clientId || !tenantId) {
  throw new Error(
    'Microsoft Entra configuration is missing.'
  )
}

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: window.location.origin + '/',
    postLogoutRedirectUri: window.location.origin + '/',
  },
  cache: {
    cacheLocation: 'sessionStorage',
  },
}

const apiScope = import.meta.env.VITE_ENTRA_API_SCOPE

if (!apiScope) {
  throw new Error('Microsoft Entra API scope is missing.')
}

export const invoiceApiScopes = [apiScope]

export const msalInstance =
  new PublicClientApplication(msalConfig)