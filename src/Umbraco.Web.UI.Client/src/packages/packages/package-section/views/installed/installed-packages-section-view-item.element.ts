import { html, css, nothing, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { PackageService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestPackageView } from '@umbraco-cms/backoffice/package';

@customElement('umb-installed-packages-section-view-item')
export class UmbInstalledPackagesSectionViewItemElement extends UmbLitElement {
	@property()
	public set name(value: string | undefined) {
		const oldValue = this.#name;
		if (oldValue === value) return;
		this.#name = value;
		this.#observePackageView();
		this.requestUpdate('name', oldValue);
	}
	public get name(): string | undefined {
		return this.#name;
	}
	#name?: string | undefined;

	@property()
	version?: string | null;

	@property({ type: Boolean, attribute: false })
	hasPendingMigrations = false;

	@property({ attribute: 'custom-icon' })
	customIcon?: string;

	@state()
	private _migrationButtonState?: UUIButtonState;

	@state()
	private _packageView?: ManifestPackageView;

	#notificationContext?: UmbNotificationContext;

	constructor() {
		super();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance;
		});
	}

	#observePackageView() {
		this.observe(
			umbExtensionsRegistry.byTypeAndFilter('packageView', (manifest) => manifest.meta.packageName === this.#name),
			(manifests) => {
				if (manifests.length === 0) {
					this._packageView = undefined;
					return;
				}
				this._packageView = manifests[0];
			},
			'_observePackageViewExtension',
		);
	}

	async #onConfigure() {
		if (!this._packageView) {
			console.warn('Tried to configure package without view');
			return;
		}

		const element = await createExtensionElement(this._packageView);

		if (!element) {
			console.warn('Failed to create package view element');
			return;
		}

		// TODO: add dedicated modal for package views, and register it in a manifest.
		throw new Error('package view modal temporarily disabled.');
		/*
		this._modalContext?.open(this, element, {
			data: { name: this.name, version: this.version },
			size: 'full',
			type: 'sidebar',
		});
		*/
	}

	async #onMigration() {
		if (!this.name) return;

		await umbConfirmModal(this, {
			color: 'positive',
			headline: this.name,
			content: this.localize.term('packager_packageMigrationsConfirmText'),
		});

		this._migrationButtonState = 'waiting';
		const { error } = await tryExecute(
			this,
			PackageService.postPackageByNameRunMigration({ path: { name: this.name } }),
		);

		if (error) return;

		this.#notificationContext?.peek('positive', {
			data: {
				message: this.localize.term('packager_packageMigrationsComplete'),
			},
		});

		this._migrationButtonState = 'success';
		this.hasPendingMigrations = false;
	}

	override render() {
		return this.name
			? html`
					<uui-ref-node-package
						name=${ifDefined(this.name)}
						version="${ifDefined(this.version ?? undefined)}"
						@open=${this.#onConfigure}
						?disabled="${!this._packageView}">
						${this.customIcon ? html`<umb-icon slot="icon" name=${this.customIcon}></umb-icon>` : nothing}
						<div slot="tag">
							${this.hasPendingMigrations
								? html`<uui-button
										@click="${this.#onMigration}"
										.state=${this._migrationButtonState}
										color="warning"
										look="primary"
										label=${this.localize.term('packager_packageMigrationsRun')}></uui-button>`
								: nothing}
						</div>
					</uui-ref-node-package>
				`
			: '';
	}

	static override styles = css`
		:host {
			display: flex;
			min-height: 47px;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installed-packages-section-view-item': UmbInstalledPackagesSectionViewItemElement;
	}
}
