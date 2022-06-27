import { BehaviorSubject, Observable, ReplaySubject, Subject } from 'rxjs';

import { getInstallSettings, postInstallSetup } from '../core/api/fetcher';
import { PostInstallRequest, UmbracoInstaller } from '../core/models';

export class UmbInstallerContext {
  private _data: BehaviorSubject<PostInstallRequest> = new BehaviorSubject<PostInstallRequest>({
    name: '',
    email: '',
    password: '',
    subscribeToNewsletter: false,
    telemetryLevel: 'Basic',
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
      postInstallSetup(this._data.getValue()).then(resolve, ({ data }) => {
        reject(data);
      });
    });
  }

  private loadIntallerSettings() {
    getInstallSettings({}).then(({ data }) => {
      this._settings.next(data);
    });
  }
}
