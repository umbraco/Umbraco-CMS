import { UMB_CREATE_MEMBER_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { css, customElement, html, map, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbMemberTypeStructureRepository } from '@umbraco-cms/backoffice/member-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/collection';
import type { UmbAllowedMemberTypeModel } from '@umbraco-cms/backoffice/member-type';

@customElement('umb-create-member-collection-action')
export class UmbCreateMemberCollectionActionElement extends UmbLitElement {
	@state()
	private _allowedMemberTypes: Array<UmbAllowedMemberTypeModel> = [];

	@state()
	private _popoverOpen = false;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	#memberTypeStructureRepository = new UmbMemberTypeStructureRepository(this);

	override async firstUpdated() {
		this.#retrieveAllowedMemberTypes();
	}

	async #retrieveAllowedMemberTypes() {
		const { data } = await this.#memberTypeStructureRepository.requestAllowedChildrenOf(null, null);
		if (data && data.items) {
			this._allowedMemberTypes = data.items;
		}
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	#getCreateUrl(item: UmbAllowedMemberTypeModel) {
		if (!item.unique) {
			throw new Error('Item unique is missing');
		}
		return UMB_CREATE_MEMBER_WORKSPACE_PATH_PATTERN.generateAbsolute({
			memberTypeUnique: item.unique,
		});
	}

	override render() {
		return this._allowedMemberTypes.length !== 1 ? this.#renderDropdown() : this.#renderCreateButton();
	}

	#renderCreateButton() {
		if (this._allowedMemberTypes.length !== 1) return;

		const item = this._allowedMemberTypes[0];
		const label =
			(this.manifest?.meta.label
				? this.localize.string(this.manifest?.meta.label)
				: this.localize.term('general_create')) +
			' ' +
			this.localize.string(item.name);

		return html`
			<uui-button color="default" href=${this.#getCreateUrl(item)} label=${label} look="outline"></uui-button>
		`;
	}

	#renderDropdown() {
		if (!this._allowedMemberTypes.length) return;

		const label = this.manifest?.meta.label
			? this.localize.string(this.manifest?.meta.label)
			: this.localize.term('general_create');

		return html`
			<uui-button popovertarget="collection-action-menu-popover" label=${label} color="default" look="outline">
				${label}
				<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container
				id="collection-action-menu-popover"
				placement="bottom-start"
				@toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						${map(
							this._allowedMemberTypes,
							(item) => html`
								<uui-menu-item label=${this.localize.string(item.name)} href=${this.#getCreateUrl(item)}>
									<umb-icon slot="icon" name=${item.icon ?? 'icon-user'}></umb-icon>
								</uui-menu-item>
							`,
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			uui-scroll-container {
				max-height: 500px;
			}
		`,
	];
}

export default UmbCreateMemberCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-member-collection-action': UmbCreateMemberCollectionActionElement;
	}
}
