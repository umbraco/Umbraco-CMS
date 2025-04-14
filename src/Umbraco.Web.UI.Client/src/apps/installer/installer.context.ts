import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type {
	InstallSettingsResponseModel,
	InstallRequestModelReadable,
} from '@umbraco-cms/backoffice/external/backend-api';
import { InstallService, TelemetryLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute, UmbApiError, type UmbProblemDetails } from '@umbraco-cms/backoffice/resources';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbObjectState, UmbNumberState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Context API for the installer
 * @class UmbInstallerContext
 */
export class UmbInstallerContext extends UmbContextBase<UmbInstallerContext, typeof UMB_INSTALLER_CONTEXT> {
	private _data = new UmbObjectState<InstallRequestModelReadable>({
		user: { name: '', email: '', password: '', subscribeToNewsletter: false },
		database: { id: '', providerName: '', useIntegratedAuthentication: false, trustServerCertificate: false },
		telemetryLevel: TelemetryLevelModel.DETAILED,
	});
	public readonly data = this._data.asObservable();

	private _currentStep = new UmbNumberState<number>(1);
	public readonly currentStep = this._currentStep.asObservable();

	private _settings = new UmbObjectState<InstallSettingsResponseModel | undefined>(undefined);
	public readonly settings = this._settings.asObservable();

	private _installStatus = new UmbObjectState<UmbProblemDetails | null>(null);
	public readonly installStatus = this._installStatus.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_INSTALLER_CONTEXT);
		this._loadInstallerSettings();
	}

	/**
	 * Observable method to get the current step in the installation process
	 * @public
	 * @returns {*}  {Observable<number>}
	 * @memberof UmbInstallerContext
	 */
	public currentStepChanges(): Observable<number> {
		return this.currentStep;
	}

	/**
	 * Observable method to get the install status in the installation process
	 * @public
	 * @returns {*}  {(Observable<UmbProblemDetails | null>)}
	 * @memberof UmbInstallerContext
	 */
	public installStatusChanges(): Observable<UmbProblemDetails | null> {
		return this.installStatus;
	}

	/**
	 * Increment the current step in the installation process
	 * @public
	 * @memberof UmbInstallerContext
	 */
	public nextStep(): void {
		this._currentStep.setValue(this._currentStep.getValue() + 1);
	}

	/**
	 * Decrements the current step in the installation process
	 * @public
	 * @memberof UmbInstallerContext
	 */
	public prevStep(): void {
		this._currentStep.setValue(this._currentStep.getValue() - 1);
	}

	/**
	 * Reset the installation process
	 * @public
	 * @memberof UmbInstallerContext
	 */
	public reset(): void {
		this._installStatus.setValue(null);
		this._currentStep.setValue(1);
	}

	/**
	 * Set the data for the installation process
	 * @public
	 * @param {Partial<InstallRequestModel>} data The data to set
	 * @memberof UmbInstallerContext
	 */
	public appendData(data: Partial<InstallRequestModelReadable>): void {
		this._data.setValue({ ...this.getData(), ...data });
	}

	/**
	 * Get the data for the installation process
	 * @public
	 * @returns {*}  {PostInstallRequest}
	 * @memberof UmbInstallerContext
	 */
	public getData(): InstallRequestModelReadable {
		return this._data.getValue();
	}

	public async postInstallSetup(): Promise<boolean> {
		const { error } = await tryExecute(this, InstallService.postInstallSetup({ body: this.getData() }), {
			disableNotifications: true,
		});
		if (error) {
			if (UmbApiError.isUmbApiError(error)) this.setInstallStatus(error.problemDetails);
			else this.setInstallStatus({ title: 'Unknown error', detail: error.message, status: 500, type: 'error' });
			return false;
		}

		// TODO: The post install will probably return a user in the future, so we have to set that context somewhere to let the client know that it is authenticated
		history.replaceState(null, '', 'section/content');

		return true;
	}

	/**
	 * Set the install status
	 * @public
	 * @param {(UmbProblemDetails | null)} status The status to set
	 * @memberof UmbInstallerContext
	 */
	public setInstallStatus(status: UmbProblemDetails | null): void {
		this._installStatus.setValue(status);
	}

	/**
	 * Load installer settings from the API
	 * @private
	 * @memberof UmbInstallerContext
	 */
	private async _loadInstallerSettings() {
		const { data, error } = await tryExecute(this, InstallService.getInstallSettings(), {
			disableNotifications: true,
		});
		if (data) {
			this._settings.setValue(data);
		} else if (UmbApiError.isUmbApiError(error)) {
			this._installStatus.setValue(error.problemDetails);
		}
	}
}

export const UMB_INSTALLER_CONTEXT = new UmbContextToken<UmbInstallerContext>('UmbInstallerContext');
