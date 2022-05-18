export interface InitResponse {
    version: string;
    installed: boolean;
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