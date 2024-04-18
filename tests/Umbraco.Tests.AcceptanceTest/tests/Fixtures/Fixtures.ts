import { test as base } from '@playwright/test';
import {Authentication} from "./Authentication";

type Fixtures = {
  authentication: Authentication;
}

export const test = base.extend<Fixtures>({

})
