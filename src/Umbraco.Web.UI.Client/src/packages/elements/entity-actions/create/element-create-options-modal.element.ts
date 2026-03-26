import type { UmbElementEntityTypeUnion } from '../../entity.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import {
	UmbElementTypeStructureRepository,
	type UmbAllowedElementTypeModel,
} from '../../repository/structure/index.js';
import type {
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue,
} from './element-create-options-modal.token.js';
import { html, nothing, customElement, state, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-element-create-options-modal')
export class UmbElementCreateOptionsModalElement extends UmbModalBaseElement<
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue
> {
	#elementTypeStructureRepository = new UmbElementTypeStructureRepository(this);

	@state()
	private _allowedElementTypes: UmbAllowedElementTypeModel[] = [];

	@state()
	private _createOptionControllers: Array<UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>> = [];

	@state()
	private _hrefList: Array<string | undefined> = [];

	override async firstUpdated() {
		this.#retrieveAllowedElementTypes();
		this.#initCreateOptionActions();
	}

	async #retrieveAllowedElementTypes() {
		const { data } = await this.#elementTypeStructureRepository.requestAllowedChildrenOf(null, null);

		if (data) {
			this._allowedElementTypes = data.items;
		}
	}

	#initCreateOptionActions() {
		const parent = this.data?.parent;
		if (!parent) return;

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(manifest: ManifestEntityCreateOptionAction) => {
				return [{ entityType: parent.entityType, unique: parent.unique, meta: manifest.meta }];
			},
			(manifest: ManifestEntityCreateOptionAction) => manifest.forEntityTypes.includes(parent.entityType),
			async (controllers) => {
				this._createOptionControllers = controllers as unknown as Array<
					UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>
				>;
				const hrefPromises = this._createOptionControllers.map((controller) => controller.api?.getHref());
				this._hrefList = await Promise.all(hrefPromises);
			},
			'umbElementCreateOptionActions',
		);
	}

	#onNavigate(elementType: UmbAllowedElementTypeModel) {
		const parentEntityType = this.data?.parent.entityType as UmbElementEntityTypeUnion;
		const parentUnique = this.data?.parent.unique ?? null;
		const documentTypeUnique = elementType.unique;
		if (!documentTypeUnique) return;

		history.pushState(
			null,
			'',
			UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
				parentEntityType,
				parentUnique,
				documentTypeUnique,
			}),
		);
		this._submitModal();
	}

	async #onClick(
		event: Event,
		controller: UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>,
		href?: string,
	) {
		if (href) return;
		event.stopPropagation();
		if (!controller.api) throw new Error('No API found');
		await controller.api.execute().catch(() => {});
		this._submitModal();
	}

	override render() {
		return html`
			<uui-dialog-layout headline=${this.localize.term('general_create')}>
				${when(
					this._allowedElementTypes.length === 0 && this._createOptionControllers.length === 0,
					() => this.#renderNoAllowedTypes(),
					() => html`${this.#renderAllowedElementTypes()}${this.#renderCreateOptionActions()}`,
				)}
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('general_cancel')}
					@click="${this._rejectModal}"></uui-button>
			</uui-dialog-layout>
		`;
	}

	#renderNoAllowedTypes() {
		return html`
			<umb-localize key="create_noElementTypes">
				There are no allowed Element Types available for creating elements here. You must enable these in
				<strong>Document Types</strong> within the <strong>Settings</strong> section, by editing the
				<strong>Allow at library root</strong> under <strong>Permissions</strong>.
			</umb-localize>
		`;
	}

	#renderAllowedElementTypes() {
		return repeat(
			this._allowedElementTypes,
			(elementType) => elementType.unique,
			(elementType) => html`
				<uui-ref-node-document-type
					.name=${this.localize.string(elementType.name) + '...'}
					.alias=${this.localize.string(elementType.description ?? '')}
					select-only
					selectable
					@selected=${() => this.#onNavigate(elementType)}>
					<umb-icon slot="icon" name=${elementType.icon ?? 'icon-document'}></umb-icon>
				</uui-ref-node-document-type>
			`,
		);
	}

	#renderCreateOptionActions() {
		return repeat(
			this._createOptionControllers,
			(controller) => controller.manifest?.alias,
			(controller, index) => {
				const manifest = controller.manifest;
				if (!manifest) return nothing;

				const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
				const href = this._hrefList[index];

				return html`
					<uui-ref-node-document-type
						.name=${manifest.meta.additionalOptions ? label + '...' : label}
						select-only
						selectable
						@selected=${(event: Event) => this.#onClick(event, controller, href)}>
						<umb-icon slot="icon" name=${manifest.meta.icon}></umb-icon>
					</uui-ref-node-document-type>
				`;
			},
		);
	}

	static override styles = [UmbTextStyles];
}

export default UmbElementCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-create-options-modal': UmbElementCreateOptionsModalElement;
	}
}
