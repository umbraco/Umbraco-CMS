import { html, css, nothing } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { customElement, property, state } from 'lit/decorators.js';
import { firstValueFrom, map } from 'rxjs';
import { UUIButtonState } from '@umbraco-ui/uui';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_CONFIRM_MODAL } from '@umbraco-cms/backoffice/modal';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';

import type { ManifestPackageView } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { PackageResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

@customElement('umb-installed-packages-section-view-item')
export class UmbInstalledPackagesSectionViewItemElement extends UmbLitElement {
	static styles = css`
		:host {
			display: flex;
			min-height: 47px;
		}
	`;

	@property()
	name?: string;

	@property()
	version?: string | null;

	@property()
	hasPendingMigrations = false;

	@property()
	customIcon?: string;

	@state()
	private _migrationButtonState?: UUIButtonState;

	@state()
	private _packageView?: ManifestPackageView;

	private _notificationContext?: UmbNotificationContext;
	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this._notificationContext = instance;
		});
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		if (this.name?.length) {
			this.findPackageView(this.name);
		}
	}

	private async findPackageView(alias: string) {
		const observable = umbExtensionsRegistry
			?.extensionsOfType('packageView')
			.pipe(map((e) => e.filter((m) => m.meta.packageName === alias)));

		if (!observable) {
			return;
		}

		const views = await firstValueFrom(observable);
		if (!views.length) {
			return;
		}

		this._packageView = views[0];
	}

	async _onMigration() {
		if (!this.name) return;
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL, {
			color: 'positive',
			headline: `Run migrations for ${this.name}?`,
			content: `Do you want to start run migrations for ${this.name}`,
			confirmLabel: 'Run migrations',
		});

		await modalHandler?.onSubmit();

		this._migrationButtonState = 'waiting';
		const { error } = await tryExecuteAndNotify(
			this,
			PackageResource.postPackageByNameRunMigration({ name: this.name })
		);
		if (error) return;
		this._notificationContext?.peek('positive', { data: { message: 'Migrations completed' } });
		this._migrationButtonState = 'success';
		this.hasPendingMigrations = false;
	}

	render() {
		return html`
			<uui-ref-node-package
				name=${ifDefined(this.name)}
				version="${ifDefined(this.version ?? undefined)}"
				@open=${this._onConfigure}
				?disabled="${!this._packageView}">
				${this.customIcon ? html`<uui-icon slot="icon" name="${this.customIcon}"></uui-icon>` : nothing}
				<div slot="tag">
					${this.hasPendingMigrations
						? html`<uui-button
								@click="${this._onMigration}"
								.state=${this._migrationButtonState}
								color="warning"
								look="primary"
								label="Run pending package migrations">
								Run pending migrations
						  </uui-button>`
						: nothing}
				</div>
			</uui-ref-node-package>
		`;
	}

	private async _onConfigure() {
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
		alert('package view modal temporarily disabled. See comment in code.');
		/*
		this._modalContext?.open(element, {
			data: { name: this.name, version: this.version },
			size: 'full',
			type: 'sidebar',
		});
		*/
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installed-packages-section-view-item': UmbInstalledPackagesSectionViewItemElement;
	}
}
