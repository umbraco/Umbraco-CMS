import { BehaviorSubject, ReplaySubject } from 'rxjs';

import { getInstallSettings, postInstallSetup } from '../core/api/fetcher';
import { PostInstallRequest, UmbracoInstaller } from '../core/models';

export class UmbInstallerContext {
	private _data = new BehaviorSubject<PostInstallRequest>({
		user: { name: '', email: '', password: '', subscribeToNewsletter: false },
		telemetryLevel: 'Basic',
	});
	public readonly data = this._data.asObservable();

	private _settings = new ReplaySubject<UmbracoInstaller>();
	public readonly settings = this._settings.asObservable();

	constructor() {
		this.loadIntallerSettings();
	}

	public appendData(data: any) {
		this._data.next({ ...this.getData(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}

	public requestInstall() {
		// TODO: The post install will probably return a user in the future, so we have to set that context somewhere to let the client know that it is authenticated
		return postInstallSetup(this.getData());
	}

	private loadIntallerSettings() {
		getInstallSettings({}).then(({ data }) => {
			this._settings.next(data);
		});
	}
}
