import { umbLoginData } from "../data/login.data.js";
import { rest } from "msw";

export const handlers = [
  rest.post(
    "http://localhost:5173/umbraco/management/api/v1/security/back-office/login",
    async (req, res, ctx) => {
      const json = await req.json();

      const username = json.username;
      const password = json.password;

      const { status, data } = umbLoginData.login(username, password);

      return res(ctx.status(status), ctx.json(data));
    }
  ),
];
