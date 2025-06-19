import { UMB_CREATE_MEMBER_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import type {
	UmbMemberCreateOptionsModalData,
	UmbMemberCreateOptionsModalValue,
} from './member-create-options-modal.token.js';
import { html, customElement, state, repeat, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import { UmbMemberTypeTreeRepository } from '@umbraco-cms/backoffice/member-type';

const elementName = 'umb-member-create-options-modal';
@customElement(elementName)
export class UmbMemberCreateOptionsModalElement extends UmbModalBaseElement<
	UmbMemberCreateOptionsModalData,
	UmbMemberCreateOptionsModalValue
> {
	@state()
	private _options: Array<{ label: string; unique: string; icon: string }> = [];

	#memberTypeTreeRepository = new UmbMemberTypeTreeRepository(this);

	override firstUpdated() {
		this.#getOptions();
	}

	async #getOptions() {
		//TODO: Should we use the tree repository or make a collection repository?
		//TODO: And how would we get all the member types?
		//TODO: This only works because member types can't have folders.
		const { data } = await this.#memberTypeTreeRepository.requestTreeRootItems({});
		if (!data) return;

		this._options = data.items.map((item) => {
			return {
				label: item.name,
				unique: item.unique,
				icon: item.icon || '',
			};
		});
	}

	// close the modal when navigating
	#onOpen(event: Event, unique: string) {
		event?.stopPropagation();
		// TODO: the href does not emit an event, so we need to use the click event
		const path = UMB_CREATE_MEMBER_WORKSPACE_PATH_PATTERN.generateAbsolute({
			memberTypeUnique: unique,
		});
		history.pushState(null, '', path);
		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_create')}>
				${this.#renderOptions()}
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('general_cancel')}
					@click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
		`;
	}

	#renderOptions() {
		return html`
			<uui-box>
				${repeat(
					this._options,
					(option) => option.unique,
					(option) => html`
						<uui-ref-node
							.name=${this.localize.string(option.label)}
							@open=${(event: Event) => this.#onOpen(event, option.unique)}>
							<umb-icon slot="icon" name=${option.icon || 'icon-circle-dotted'}></umb-icon>
						</uui-ref-node>
					`,
				)}
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#blank {
				border-bottom: 1px solid var(--uui-color-border);
			}

			#edit-permissions {
				margin-top: var(--uui-size-6);
			}
		`,
	];
}

export { UmbMemberCreateOptionsModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMemberCreateOptionsModalElement;
	}
}
