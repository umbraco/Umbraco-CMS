export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export interface IUmbAuthContext {
	login(data: LoginRequestModel): Promise<{ error?: string }>;
}

export type ProblemDetails = {
	type: string;
	title: string;
	message: string;
};
