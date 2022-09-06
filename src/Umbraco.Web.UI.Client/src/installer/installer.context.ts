import { BehaviorSubject, Observable, ReplaySubject } from 'rxjs';

import { getInstallSettings, postInstallSetup } from '../core/api/fetcher';
import type { PostInstallRequest, ProblemDetails, UmbracoInstaller } from '../core/models';

/**
 * Context API for the installer
 * @export
 * @class UmbInstallerContext
 */
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

	/**
	 * Observable method to get the current step in the installation process
	 * @public
	 * @return {*}  {Observable<number>}
	 * @memberof UmbInstallerContext
	 */
	public currentStepChanges(): Observable<number> {
		return this.currentStep;
	}

	/**
	 * Observable method to get the install status in the installation process
	 * @public
	 * @return {*}  {(Observable<ProblemDetails | null>)}
	 * @memberof UmbInstallerContext
	 */
	public installStatusChanges(): Observable<ProblemDetails | null> {
		return this.installStatus;
	}

	/**
	 * Increment the current step in the installation process
	 * @public
	 * @memberof UmbInstallerContext
	 */
	public nextStep(): void {
		this._currentStep.next(this._currentStep.getValue() + 1);
	}

	/**
	 * Decrements the current step in the installation process
	 * @public
	 * @memberof UmbInstallerContext
	 */
	public prevStep(): void {
		this._currentStep.next(this._currentStep.getValue() - 1);
	}

	/**
	 * Reset the installation process
	 * @public
	 * @memberof UmbInstallerContext
	 */
	public reset(): void {
		this._currentStep.next(1);
		this._installStatus.next(null);
	}

	/**
	 * Set the data for the installation process
	 * @public
	 * @param {Partial<PostInstallRequest>} data
	 * @memberof UmbInstallerContext
	 */
	public appendData(data: Partial<PostInstallRequest>): void {
		this._data.next({ ...this.getData(), ...data });
	}

	/**
	 * Get the data for the installation process
	 * @public
	 * @return {*}  {PostInstallRequest}
	 * @memberof UmbInstallerContext
	 */
	public getData(): PostInstallRequest {
		return this._data.getValue();
	}

	/**
	 * Post the installation data to the API
	 * @public
	 * @return {*}
	 * @memberof UmbInstallerContext
	 */
	public requestInstall() {
		// TODO: The post install will probably return a user in the future, so we have to set that context somewhere to let the client know that it is authenticated
		return postInstallSetup(this.getData());
	}

	/**
	 * Set the install status
	 * @public
	 * @param {(ProblemDetails | null)} status
	 * @memberof UmbInstallerContext
	 */
	public setInstallStatus(status: ProblemDetails | null): void {
		this._installStatus.next(status);
	}

	/**
	 * Load installer settings from the API
	 * @private
	 * @memberof UmbInstallerContext
	 */
	private _loadInstallerSettings() {
		getInstallSettings({}).then(({ data }) => {
			this._settings.next(data);
		});
	}
}
