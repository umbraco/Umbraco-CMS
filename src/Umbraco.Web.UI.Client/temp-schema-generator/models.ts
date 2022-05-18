export interface InitResponse {
    version: string;
    installed: boolean;
}

export interface UserResponse {
    username: string;
}

export interface UserLoginRequest {
    userame: string;
    password: string;
    role: string;
}

export interface ErrorResponse {
    errorMessage: string;
}