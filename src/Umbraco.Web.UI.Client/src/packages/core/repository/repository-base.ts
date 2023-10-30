import { UmbBaseController, UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";

export class UmbRepositoryBase extends UmbBaseController {
  constructor(host: UmbControllerHostElement) {
    super(host);
  }
}