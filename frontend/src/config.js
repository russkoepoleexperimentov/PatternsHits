const config = {
  authority: 'http://localhost:2280', // URL IdentityServer
  client_id: 'web_client', // Client ID, зарегистрированный в IdentityServer
  redirect_uri: 'http://localhost:3000/signin-oidc', // Адрес после логина
  response_type: 'code',
  scope: 'openid profile account_api credit_api',
  post_logout_redirect_uri: 'http://localhost:3000/signout-callback-oidc',
};

export default config;