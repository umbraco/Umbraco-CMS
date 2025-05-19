import { css, customElement, html, ifDefined, property, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

export type UmbCascadingMenuItem = {
	label: string;
	icon?: string;
	items?: Array<UmbCascadingMenuItem>;
	element?: HTMLElement;
	separatorAfter?: boolean;
	style?: string;
	execute?: () => void;
};

@customElement('umb-cascading-menu-popover')
export class UmbCascadingMenuPopoverElement extends UmbElementMixin(UUIPopoverContainerElement) {
	@property({ type: Array })
	items?: Array<UmbCascadingMenuItem>;

	#getPopoverById(popoverId: string): UUIPopoverContainerElement | null | undefined {
		return this.shadowRoot?.querySelector(`#${popoverId}`) as UUIPopoverContainerElement;
	}

	#onMouseEnter(item: UmbCascadingMenuItem, popoverId: string) {
		if (!item.items?.length) return;

		const popover = this.#getPopoverById(popoverId);
		if (!popover) return;

		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		popover.showPopover();
	}

	#onMouseLeave(item: UmbCascadingMenuItem, popoverId: string) {
		const popover = this.#getPopoverById(popoverId);
		if (!popover) return;

		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		popover.hidePopover();
	}

	#onClick(item: UmbCascadingMenuItem, popoverId: string) {
		item.execute?.();

		setTimeout(() => {
			this.#onMouseLeave(item, popoverId);
		}, 100);
	}

	override render() {
		return html`
			<uui-scroll-container>
				${when(
					this.items?.length,
					() => html`${repeat(this.items!, (item, index) => this.#renderItem(item, index))} ${super.render()}`,
					() => super.render(),
				)}
			</uui-scroll-container>
		`;
	}

	#renderItem(item: UmbCascadingMenuItem, index: number) {
		const popoverId = `item-${index}`;

		const element = item.element;
		if (element) {
			element.setAttribute('popovertarget', popoverId);
		}

		const label = this.localize.string(item.label);

		return html`
			<div
				@mouseenter=${() => this.#onMouseEnter(item, popoverId)}
				@mouseleave=${() => this.#onMouseLeave(item, popoverId)}>
				${when(
					element,
					() => element,
					() => html`
						<uui-menu-item
							class=${item.separatorAfter ? 'separator' : ''}
							label=${label}
							popovertarget=${popoverId}
							@click-label=${() => this.#onClick(item, popoverId)}>
							${when(item.icon, (icon) => html`<uui-icon slot="icon" name=${icon}></uui-icon>`)}
							<div slot="label" class="menu-item">
								<span style=${ifDefined(item.style)}>${label}</span>
								${when(item.items, () => html`<uui-symbol-expand></uui-symbol-expand>`)}
							</div>
						</uui-menu-item>
					`,
				)}
				<umb-cascading-menu-popover id=${popoverId} placement="right-start" .items=${item.items}>
				</umb-cascading-menu-popover>
			</div>
		`;
	}

	static override readonly styles = [
		...UUIPopoverContainerElement.styles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;

				background-color: var(--uui-color-surface);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
				padding: var(--uui-size-space-1);
			}

			.menu-item {
				flex: 1;
				display: flex;
				justify-content: space-between;
				align-items: center;
				gap: var(--uui-size-space-4);
			}

			.separator::after {
				content: '';
				position: absolute;
				border-bottom: 1px solid var(--uui-color-border);
				width: 100%;
			}

			uui-scroll-container {
				max-height: 500px;
			}
		`,
	];
}

export default UmbCascadingMenuPopoverElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-cascading-menu-popover': UmbCascadingMenuPopoverElement;
	}
}
