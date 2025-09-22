import type { UmbTiptapToolbarExtension } from '../types.js';
import { UmbTiptapToolbarConfigurationContext } from './tiptap-toolbar-configuration.context.js';
import { css, customElement, html, property, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController, UmbSorterResolvePlacementAsGrid } from '@umbraco-cms/backoffice/sorter';

@customElement('umb-tiptap-toolbar-group-configuration')
export class UmbTiptapToolbarGroupConfigurationElement<
	TiptapToolbarItem extends UmbTiptapToolbarExtension = UmbTiptapToolbarExtension,
> extends UmbLitElement {
	#sorter = new UmbSorterController<TiptapToolbarItem, HTMLElement>(this, {
		getUniqueOfElement: (element) => element.getAttribute('tiptap-toolbar-alias'),
		getUniqueOfModel: (modelEntry) => modelEntry.alias!,
		itemSelector: 'uui-button',
		identifier: 'umb-tiptap-toolbar-sorter',
		containerSelector: '.items',
		resolvePlacement: UmbSorterResolvePlacementAsGrid,
		onContainerChange: ({ item, model }) => {
			this.dispatchEvent(new CustomEvent('container-change', { detail: { item, model } }));
		},
		onChange: ({ model }) => {
			this.#items = model;
			this.requestUpdate();
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	#context = new UmbTiptapToolbarConfigurationContext(this);

	@property({ type: Array, attribute: false })
	public set items(value: Array<TiptapToolbarItem> | undefined) {
		this.#items = (value ?? []).filter((item, index, self) => self.findIndex((x) => x.alias === item.alias) === index);
		this.#sorter.setModel(this.#items);
	}
	public get items(): Array<TiptapToolbarItem> {
		return this.#items;
	}
	#items: Array<TiptapToolbarItem> = [];

	@property({ type: Number })
	rowIndex = 0;

	@property({ type: Number })
	groupIndex = 0;

	#onRequestRemove(item: TiptapToolbarItem, index = 0) {
		this.items = this.items.filter((x) => x.alias !== item.alias);
		const rowIndex = this.rowIndex;
		const groupIndex = this.groupIndex;
		this.dispatchEvent(
			new CustomEvent('remove', { detail: { rowIndex, groupIndex, index }, bubbles: true, composed: true }),
		);
	}

	override render() {
		return html`
			<div class="items">
				${when(
					this.items?.length === 0,
					() => html`<em><umb-localize key="tiptap_toolbar_emptyGroup">Empty</umb-localize></em>`,
					() =>
						repeat(
							this.items,
							(item) => item.alias,
							(item, index) => this.#renderItem(item, index),
						),
				)}
			</div>
		`;
	}

	#renderItem(item: TiptapToolbarItem, index = 0) {
		const label = this.localize.string(item.label);
		const forbidden = !this.#context?.isExtensionEnabled(item.alias);

		switch (item.kind) {
			case 'styleMenu':
			case 'menu':
				return html`
					<uui-button
						compact
						class=${forbidden ? 'forbidden' : ''}
						data-mark="tiptap-toolbar-item:${item.alias}"
						look=${forbidden ? 'placeholder' : 'outline'}
						label=${label}
						title=${label}
						?disabled=${forbidden}
						tiptap-toolbar-alias=${item.alias}
						@click=${() => this.#onRequestRemove(item, index)}>
						<div class="inner">
							<span>${label}</span>
						</div>
						<uui-symbol-expand slot="extra" open></uui-symbol-expand>
					</uui-button>
				`;

			case 'button':
			default:
				return html`
					<uui-button
						compact
						class=${forbidden ? 'forbidden' : ''}
						data-mark="tiptap-toolbar-item:${item.alias}"
						look=${forbidden ? 'placeholder' : 'outline'}
						label=${label}
						title=${label}
						?disabled=${forbidden}
						tiptap-toolbar-alias=${item.alias}
						@click=${() => this.#onRequestRemove(item, index)}>
						<div class="inner">
							${when(
								item.icon,
								() => html`<umb-icon .name=${item.icon}></umb-icon>`,
								() => html`<span>${label}</span>`,
							)}
						</div>
					</uui-button>
				`;
		}
	}

	static override styles = [
		css`
			.items {
				display: flex;
				flex-direction: row;
				flex-wrap: wrap;
				gap: var(--uui-size-1);

				uui-button {
					--uui-button-font-weight: normal;

					&[draggable='true'],
					&[draggable='true'] > .inner {
						cursor: move;
					}

					&[disabled],
					&[disabled] > .inner {
						cursor: not-allowed;
					}

					&.forbidden {
						--color: var(--uui-color-danger);
						--color-standalone: var(--uui-color-danger-standalone);
						--color-emphasis: var(--uui-color-danger-emphasis);
						--color-contrast: var(--uui-color-danger);
						--uui-button-contrast-disabled: var(--uui-color-danger);
						--uui-button-border-color-disabled: var(--uui-color-danger);
					}

					div {
						display: flex;
						gap: var(--uui-size-1);
						align-items: flex-end;
					}

					uui-symbol-expand {
						margin-left: var(--uui-size-space-2);
					}
				}
			}

			uui-button[look='outline'] {
				--uui-button-background-color-hover: var(--uui-color-surface);
			}
		`,
	];
}

export default UmbTiptapToolbarGroupConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-group-configuration': UmbTiptapToolbarGroupConfigurationElement;
	}
}
