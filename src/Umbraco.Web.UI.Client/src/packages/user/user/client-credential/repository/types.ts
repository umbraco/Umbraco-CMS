export interface UmbCreateUserClientCredentialRequestArgs {
	user: { unique: string };
	client: { unique: string; secret: string };
}

export interface UmbUserClientCredentialRequestArgs {
	user: { unique: string };
}

export interface UmbDeleteUserClientCredentialRequestArgs {
	user: { unique: string };
	client: { unique: string };
}

export interface UmbUserClientCredentialModel {
	unique: string;
}
