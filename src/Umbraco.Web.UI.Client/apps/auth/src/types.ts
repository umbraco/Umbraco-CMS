export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export interface UmbAuthContext {
	authUrl: string;
	login(data: LoginRequestModel): Promise<{ error?: string }>;
}
