import { getInstall, postInstall } from '../core/api/fetcher';
import { PostInstallRequest, UmbracoInstaller } from '../core/models';
import { ApiResponse } from 'openapi-typescript-fetch';
import { BehaviorSubject, Observable, ReplaySubject, Subject, timeout } from 'rxjs';

export class UmbInstallerContext {
  private _data: BehaviorSubject<PostInstallRequest> = new BehaviorSubject<PostInstallRequest>({
    name: '',
    email: '',
    password: '',
    subscribeToNewsletter: false,
    telemetryLevel: 'Minimal',
    database: {},
  });
  public readonly data: Observable<PostInstallRequest> = this._data.asObservable();

  private _settings: Subject<UmbracoInstaller> = new ReplaySubject<UmbracoInstaller>();
  public readonly settings: Observable<UmbracoInstaller> = this._settings.asObservable();

  constructor() {
    this.loadIntallerSettings();
  }

  public appendData(data: any) {
    this._data.next({ ...this._data.getValue(), ...data });
  }

  public getData() {
    return this._data.getValue();
  }

  public requestInstall() {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        // simulate 2 sec delay
        postInstall(this._data.getValue()).then(resolve, reject);
      }, 2000);
    });
  }

  private loadIntallerSettings() {
    getInstall({}).then(({ data }) => {
      this._settings.next(data);
      console.log('INSTALLER SETTINGS', data);
    });
  }
}
