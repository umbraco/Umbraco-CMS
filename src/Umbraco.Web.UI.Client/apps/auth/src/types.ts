export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export interface UmbAuthContext {
	returnUrl: string;
	authUrl: string;
	login(data: LoginRequestModel): Promise<void>;
}
