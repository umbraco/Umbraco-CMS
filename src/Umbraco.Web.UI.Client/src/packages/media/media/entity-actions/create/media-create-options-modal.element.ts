import { UmbMediaItemRepository } from '../../repository/index.js';
import type {
	UmbMediaCreateOptionsModalData,
	UmbMediaCreateOptionsModalValue,
} from './media-create-options-modal.token.js';
import { html, nothing, customElement, state, repeat, css, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbMediaTypeStructureRepository, type UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-media-create-options-modal')
export class UmbMediaCreateOptionsModalElement extends UmbModalBaseElement<
	UmbMediaCreateOptionsModalData,
	UmbMediaCreateOptionsModalValue
> {
	#mediaTypeStructureRepository = new UmbMediaTypeStructureRepository(this);
	#mediaItemRepository = new UmbMediaItemRepository(this);

	@state()
	private _allowedMediaTypes: UmbAllowedMediaTypeModel[] = [];

	@state()
	private _headline: string = this.localize.term('general_create');

	override async firstUpdated() {
		const mediaUnique = this.data?.parent.unique;
		const mediaTypeUnique = this.data?.mediaType?.unique || null;

		this.#retrieveAllowedMediaTypesOf(mediaTypeUnique, mediaUnique || null);

		if (mediaUnique) {
			this.#retrieveHeadline(mediaUnique);
		}
	}

	async #retrieveAllowedMediaTypesOf(unique: string | null, parentContentUnique: string | null) {
		const { data } = await this.#mediaTypeStructureRepository.requestAllowedChildrenOf(unique, parentContentUnique);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedMediaTypes = data.items;
		}
	}

	async #retrieveHeadline(unique: string) {
		if (!unique) return;
		const { data } = await this.#mediaItemRepository.requestItems([unique]);
		if (data) {
			// TODO: we need to get the correct variant context here
			this._headline = this.localize.term('create_createUnder') + ' ' + data[0].variants?.[0].name;
		}
	}

	// close the modal when navigating to media
	#onNavigate(mediaType: UmbAllowedMediaTypeModel) {
		// TODO: Use a URL builder instead of hardcoding the URL. [NL]
		const url = `section/media/workspace/media/create/parent/${this.data?.parent.entityType}/${
			this.data?.parent.unique ?? 'null'
		}/${mediaType.unique}`;
		history.pushState({}, '', url);
		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this._headline ?? ''}>
				<uui-box>
					${when(
						this._allowedMediaTypes.length === 0,
						() => this.#renderNotAllowed(),
						() => this.#renderAllowedMediaTypes(),
					)}
				</uui-box>
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('general_cancel')}
					@click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
		`;
	}

	#renderNotAllowed() {
		return html`
			<umb-localize key="create_noMediaTypes">
				There are no allowed Media Types available for creating media here. You must enable these in
				<strong>Media Types</strong> within the <strong>Settings</strong> section, by editing the
				<strong>Allowed child node types</strong> under <strong>Permissions</strong>. </umb-localize
			><br />
			<uui-button
				id="edit-permissions"
				look="secondary"
				@click=${() => this._rejectModal()}
				href=${`/section/settings/workspace/media-type/edit/${this.data?.mediaType?.unique}/view/structure`}
				label=${this.localize.term('create_noMediaTypesEditPermissions')}></uui-button>
		`;
	}

	#renderAllowedMediaTypes() {
		return repeat(
			this._allowedMediaTypes,
			(mediaType) => mediaType.unique,
			(mediaType) => html`
				<uui-ref-node-document-type
					.name=${this.localize.string(mediaType.name)}
					.alias=${this.localize.string(mediaType.description ?? '')}
					select-only
					selectable
					@selected=${() => this.#onNavigate(mediaType)}>
					${mediaType.icon ? html`<umb-icon slot="icon" name=${mediaType.icon}></umb-icon>` : nothing}
				</uui-ref-node-document-type>
			`,
		);
	}

	static override styles = [
		UmbTextStyles,
		css`
			#edit-permissions {
				margin-top: var(--uui-size-6);
			}
		`,
	];
}

export default UmbMediaCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-create-options-modal': UmbMediaCreateOptionsModalElement;
	}
}
