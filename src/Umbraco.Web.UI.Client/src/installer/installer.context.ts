import { Observable } from 'rxjs';
import {
	Install,
	InstallResource,
	InstallSettings,
	ProblemDetailsModel,
	TelemetryLevel,
} from '@umbraco-cms/backend-api';
import { tryExecute } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ObjectState, NumberState } from '@umbraco-cms/observable-api';

/**
 * Context API for the installer
 * @export
 * @class UmbInstallerContext
 */
export class UmbInstallerContext {
	private _data = new ObjectState<Install>({
		user: { name: '', email: '', password: '', subscribeToNewsletter: false },
		database: { id: '', providerName: '' },
		telemetryLevel: TelemetryLevel.BASIC,
	});
	public readonly data = this._data.asObservable();

	private _currentStep = new NumberState<number>(1);
	public readonly currentStep = this._currentStep.asObservable();

	private _settings = new ObjectState<InstallSettings | undefined>(undefined);
	public readonly settings = this._settings.asObservable();

	private _installStatus = new ObjectState<ProblemDetailsModel | null>(null);
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
	 * @return {*}  {(Observable<ProblemDetailsModel | null>)}
	 * @memberof UmbInstallerContext
	 */
	public installStatusChanges(): Observable<ProblemDetailsModel | null> {
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
		this._installStatus.next(null);
		this._currentStep.next(1);
	}

	/**
	 * Set the data for the installation process
	 * @public
	 * @param {Partial<PostInstallRequest>} data
	 * @memberof UmbInstallerContext
	 */
	public appendData(data: Partial<Install>): void {
		this._data.next({ ...this.getData(), ...data });
	}

	/**
	 * Get the data for the installation process
	 * @public
	 * @return {*}  {PostInstallRequest}
	 * @memberof UmbInstallerContext
	 */
	public getData(): Install {
		return this._data.getValue();
	}

	/**
	 * Set the install status
	 * @public
	 * @param {(ProblemDetailsModel | null)} status
	 * @memberof UmbInstallerContext
	 */
	public setInstallStatus(status: ProblemDetailsModel | null): void {
		this._installStatus.next(status);
	}

	/**
	 * Load installer settings from the API
	 * @private
	 * @memberof UmbInstallerContext
	 */
	private async _loadInstallerSettings() {
		const { data, error } = await tryExecute(InstallResource.getInstallSettings());
		if (data) {
			this._settings.next(data);
		} else if (error) {
			console.error(error.detail, error);
			this._installStatus.next(error);
		}
	}
}

export const UMB_INSTALLER_CONTEXT_TOKEN = new UmbContextToken<UmbInstallerContext>(UmbInstallerContext.name);
