import { customElement, html, ifDefined, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTreeCreateActionApi } from './tree-create-action.api.js';
import type { UmbTreeCreateOption } from './types.js';

@customElement('umb-tree-create-action-button')
export class UmbTreeCreateActionButtonElement extends UmbLitElement {
	@state()
	private _popoverOpen = false;

	@state()
	private _multipleOptions = false;

	@state()
	private _options: Array<UmbTreeCreateOption> = [];

	#createLabel = this.localize.term('general_create');
	#api: UmbTreeCreateActionApi | undefined;

	public get api(): UmbTreeCreateActionApi | undefined {
		return this.#api;
	}
	public set api(value: UmbTreeCreateActionApi | undefined) {
		this.#api = value;

		this.observe(this.#api?.options, (options) => {
			this._options = options ?? [];
		});

		this.observe(this.#api?.multipleOptions, (multipleOptions) => {
			this._multipleOptions = multipleOptions ?? false;
		});
	}

	#onPopoverToggle(event: Event) {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS yet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	async #onClick(event: Event, alias: string, href?: string) {
		if (href) return;
		event.stopPropagation();
		await this.#api?.executeByAlias(alias).catch(() => {});
	}

	#getTarget(href?: string) {
		if (href?.startsWith('http')) return '_blank';
		return '_self';
	}

	override render() {
		if (this._options.length === 0) return nothing;
		return this._multipleOptions ? this.#renderMultiOptionAction() : this.#renderSingleOptionAction();
	}

	#renderSingleOptionAction() {
		const option = this._options[0];
		return html`
			<uui-button
				label=${this.#createLabel}
				color="default"
				look="outline"
				href=${ifDefined(option?.href)}
				target=${this.#getTarget(option?.href)}
				@click=${(event: Event) => this.#onClick(event, option?.alias, option?.href)}></uui-button>
		`;
	}

	#renderMultiOptionAction() {
		return html`
			<uui-button
				popovertarget="tree-create-action-menu-popover"
				label=${this.#createLabel}
				color="default"
				look="outline">
				${this.#createLabel}
				<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			${this.#renderDropdown()}
		`;
	}

	#renderDropdown() {
		return html`
			<uui-popover-container
				id="tree-create-action-menu-popover"
				placement="bottom-start"
				@toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						${this._options.map((option) => this.#renderMenuItem(option))}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderMenuItem(option: UmbTreeCreateOption) {
		const label = option.label ? this.localize.string(option.label) : option.alias;
		return html`
			<uui-menu-item
				label=${option.additionalOptions ? label + '...' : label}
				href=${ifDefined(option.href)}
				target=${this.#getTarget(option.href)}
				@click=${(event: Event) => this.#onClick(event, option.alias, option.href)}>
				<umb-icon slot="icon" .name=${option.icon}></umb-icon>
			</uui-menu-item>
		`;
	}
}

export { UmbTreeCreateActionButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-create-action-button': UmbTreeCreateActionButtonElement;
	}
}
