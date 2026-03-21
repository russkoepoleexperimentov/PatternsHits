import { UserManager, WebStorageStateStore } from 'oidc-client-ts';
import config from '../config';

const userManager = new UserManager({
  ...config,
  userStore: new WebStorageStateStore({ store: window.localStorage })
});

export const getUser = async () => await userManager.getUser();

export const login = () => userManager.signinRedirect();

export const loginCallback = () => userManager.signinRedirectCallback();

export const logout = () => userManager.signoutRedirect();

export const renewToken = () => userManager.signinSilent();

export default userManager;