import type { UmbTiptapToolbarExtension } from '../../types.js';
import type { UmbTiptapToolbarValue } from '../../../../components/types.js';
import { UmbTiptapToolbarConfigurationContext } from '../../contexts/tiptap-toolbar-configuration.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { property, state, customElement, html, when, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSorterController, UmbSorterResolvePlacementAsGrid } from '@umbraco-cms/backoffice/sorter';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-tiptap-toolbar-group')
export class UmbTiptapToolbarGroupElement<
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
			this._value = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	#context = new UmbTiptapToolbarConfigurationContext(this);

	@property({ type: Array, attribute: false })
	public set value(items) {
		this._value = items ?? [];
		this._value = this._value.filter((value, index, self) => self.findIndex((x) => x.alias === value.alias) === index);
		this.#sorter.setModel(this._value);
	}
	public get value() {
		return this._value;
	}

	@property({ attribute: false })
	set toolbarValue(value: UmbTiptapToolbarValue | undefined) {
		if (!value) value = [[[]]];
		if (value === this.#toolbarValue) return;
		this.#toolbarValue = this.#context.isValidToolbarValue(value) ? value : [[[]]];
	}
	get toolbarValue(): UmbTiptapToolbarValue | undefined {
		if (this.#toolbarValue === undefined) return undefined;
		return this.#context.cloneToolbarValue(this.#toolbarValue);
	}
	#toolbarValue?: UmbTiptapToolbarValue;

	@property({ type: Number })
	rowIndex = 0;

	@property({ type: Number })
	groupIndex = 0;

	@state()
	private _value: Array<TiptapToolbarItem> = [];

	constructor() {
		super();
	}

	protected override firstUpdated() {
		this.#context.setToolbar(this.toolbarValue);
	}

	override render() {
		return html`
			<div class="items">
				${when(
					this.value?.length === 0,
					() => html`<em><umb-localize key="tiptap_toolbar_emptyGroup">Empty</umb-localize></em>`,
					() =>
						html`${repeat(
							this.value,
							(toolbarItem) => toolbarItem.alias,
							(item, index) => this.#renderItem(item, index),
						)}`,
				)}
			</div>
		`;
	}

	#onRequestRemove(item: TiptapToolbarItem, index = 0) {
		this.value = this.value.filter((x) => x.alias !== item.alias);
		const rowIndex = this.rowIndex;
		const groupIndex = this.groupIndex;
		this.dispatchEvent(
			new CustomEvent('toolbar-item-click', { detail: { rowIndex, groupIndex, index }, bubbles: true, composed: true }),
		);
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
}
export default UmbTiptapToolbarGroupElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-group': UmbTiptapToolbarGroupElement;
	}
}
