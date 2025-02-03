import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../media-workspace.context-token.js';
import { TimeOptions } from '../../../audit-log/info-app/utils.js';
import { css, customElement, html, ifDefined, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbMediaTypeItemModel } from '@umbraco-cms/backoffice/media-type';
import { UMB_MEDIA_TYPE_ENTITY_TYPE, UmbMediaTypeItemRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

@customElement('umb-media-workspace-view-info')
export class UmbMediaWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _mediaTypeUnique: string | undefined = undefined;

	@state()
	private _mediaTypeName?: UmbMediaTypeItemModel['name'];

	@state()
	private _mediaTypeIcon?: UmbMediaTypeItemModel['icon'];

	@state()
	private _editMediaTypePath = '';

	@state()
	private _mediaUnique = '';

	#workspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;

	#mediaTypeItemRepository = new UmbMediaTypeItemRepository(this);

	@state()
	private _createDate?: string | null = null;

	@state()
	private _updateDate?: string | null = null;

	@state()
	private _hasSettingsAccess: boolean = false;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media-type')
			.onSetup(() => {
				return { data: { entityType: UMB_MEDIA_TYPE_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaTypePath = routeBuilder({});
			});

		createExtensionApiByAlias(this, UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					match: UMB_SETTINGS_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._hasSettingsAccess = permitted;
				},
			},
		]);

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this._mediaTypeUnique = this.#workspaceContext.getContentTypeId()!;
			this.#getData();
			this.#observeContent();
		});
	}

	async #getData() {
		if (!this._mediaTypeUnique) throw new Error('Media type unique is not set');
		const { data } = await this.#mediaTypeItemRepository.requestItems([this._mediaTypeUnique]);
		this._mediaTypeName = data?.[0].name;
		this._mediaTypeIcon = data?.[0].icon;
	}

	#observeContent() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.unique,
			(unique) => {
				this._mediaUnique = unique!;
			},
			'_mediaUnique',
		);

		/** TODO: Doubt this is the right way to get the create date... */
		this.observe(this.#workspaceContext.variants, (variants) => {
			this._createDate = variants?.[0]?.createDate;
			this._updateDate = variants?.[0]?.updateDate;
		});
	}

	override render() {
		return html`
			<div class="container">
				<umb-extension-slot id="workspace-info-apps" type="workspaceInfoApp"></umb-extension-slot>
			</div>
			<div class="container">
				<uui-box headline=${this.localize.term('general_general')} id="general-section"
					>${this.#renderGeneralSection()}</uui-box
				>
			</div>
		`;
	}

	#renderGeneralSection() {
		return html`
			${this.#renderCreateDate()} ${this.#renderUpdateDate()}
			<div class="general-item">
				<strong><umb-localize key="content_mediaType">Media Type</umb-localize></strong>
				<uui-ref-node-document-type
					standalone
					href=${ifDefined(
						this._hasSettingsAccess ? this._editMediaTypePath + 'edit/' + this._mediaTypeUnique : undefined,
					)}
					?readonly=${!this._hasSettingsAccess}
					name=${ifDefined(this._mediaTypeName)}>
					${this._mediaTypeIcon ? html`<umb-icon slot="icon" name=${this._mediaTypeIcon}></umb-icon>` : nothing}
				</uui-ref-node-document-type>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_id">Id</umb-localize></strong>
				<span>${this._mediaUnique}</span>
			</div>
		`;
	}

	#renderCreateDate() {
		if (!this._createDate) return nothing;
		return html`
			<div class="general-item">
				<strong><umb-localize key="content_createDate"></umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._createDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
		`;
	}

	#renderUpdateDate() {
		if (!this._updateDate) return nothing;
		return html`
			<div class="general-item">
				<strong><umb-localize key="content_updateDate"></umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._updateDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
				grid-template-columns: 1fr 350px;
			}

			div.container {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
			}

			//General section

			#general-section {
				display: flex;
				flex-direction: column;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
			}

			uui-ref-node-document-type[readonly] {
				padding-top: 7px;
				padding-bottom: 7px;
			}
		`,
	];
}

export default UmbMediaWorkspaceViewInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-info': UmbMediaWorkspaceViewInfoElement;
	}
}
