import { BehaviorSubject, ReplaySubject } from 'rxjs';

import { getInstallSettings, postInstallSetup } from '../core/api/fetcher';
import type { PostInstallRequest, ProblemDetails, UmbracoInstaller } from '../core/models';

export class UmbInstallerContext {
	private _data = new BehaviorSubject<PostInstallRequest>({
		user: { name: '', email: '', password: '', subscribeToNewsletter: false },
		telemetryLevel: 'Basic',
	});
	public readonly data = this._data.asObservable();

	private _currentStep = new BehaviorSubject<number>(1);
	public readonly currentStep = this._currentStep.asObservable();

	private _settings = new ReplaySubject<UmbracoInstaller>();
	public readonly settings = this._settings.asObservable();

	private _installStatus = new ReplaySubject<ProblemDetails | null>(1);
	public readonly installStatus = this._installStatus.asObservable();

	constructor() {
		this._loadInstallerSettings();
	}

	public currentStepChanges() {
		return this._currentStep;
	}

	public installStatusChanges() {
		return this._installStatus;
	}

	public nextStep() {
		this._currentStep.next(this._currentStep.getValue() + 1);
	}

	public prevStep() {
		this._currentStep.next(this._currentStep.getValue() - 1);
	}

	public reset() {
		this._currentStep.next(1);
		this._installStatus.next(null);
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

	public setInstallStatus(status: ProblemDetails | null) {
		this._installStatus.next(status);
	}

	private _loadInstallerSettings() {
		getInstallSettings({}).then(({ data }) => {
			this._settings.next(data);
		});
	}
}
