export type LoginRequestModel = {
  username: string;
  password: string;
  persist: boolean;
};

export interface IUmbAuthContext {
  login(data: LoginRequestModel): Promise<LoginResponse>;
  supportsPersistLogin: boolean;
}

export type LoginResponse = {
  data?: string;
  error?: string;
  status: number;
};
