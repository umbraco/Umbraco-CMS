export interface InitResponse {
    installed: boolean;
}

export interface VersionResponse {
    version: string;
}

export interface UserResponse {
    username: string;
    role: string;
}

export interface UserLoginRequest {
    username: string;
    password: string;
    persist: boolean;
}

export interface ErrorResponse {
    errorMessage: string;
}