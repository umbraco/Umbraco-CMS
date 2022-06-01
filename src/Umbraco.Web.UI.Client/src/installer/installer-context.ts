import { getInstall } from '../core/api/fetcher';
import { PostInstallRequest, UmbracoInstaller, UmbracoPerformInstallRequest } from '../core/models';
import { ApiResponse } from 'openapi-typescript-fetch';

export class UmbInstallerContext {
  private _data: PostInstallRequest = {
    name: 'Test Name',
    email: 'emails@test',
    password: 'yyoyoy',
    subscribeToNewsletter: true,
    telemetryLevel: 'Basic',
    database: {},
  };

  private _installerSettings!: UmbracoInstaller;

  constructor() {
    this.loadIntallerSettings();
  }

  public loadIntallerSettings() {
    getInstall({}).then(({ data }) => {
      this._installerSettings = data;
    });
  }

  public getInstallerSettings() {
    return this._installerSettings;
  }

  public appendData(data: object) {
    this._data = { ...this._data, ...data };
  }

  public getData() {
    return this._data;
  }

  public requestInstall() {
    return new Promise((r) => setTimeout(r, 2000)); // wait for 2 seconds
    // try {
    //     await new Promise((r) => setTimeout(r, 2000)); // wait for 2 seconds
    //     postInstall(data).then(this._onInstallRequestFulfilled.bind(this), this._onInstallRequestRejected.bind(this));
    //   } catch (error) {
    //     console.log(error);
    //   }
  }
}
