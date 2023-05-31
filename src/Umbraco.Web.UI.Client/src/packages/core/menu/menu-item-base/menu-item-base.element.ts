import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-menu-item-base')
export class UmbMenuItemBaseElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	public entityType?: string;

	@property({ type: String, attribute: 'icon-name' })
	public iconName = '';

	@property({ type: String })
	public label = '';

	@property({ type: Boolean, attribute: 'has-children' })
	public hasChildren = false;

	@state()
	private _href?: string;

	#sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this.#sectionContext = sectionContext;
			this._observeSection();
		});
	}

	private _observeSection() {
		if (!this.#sectionContext) return;

		this.observe(this.#sectionContext?.pathname, (pathname) => {
			if (!pathname) return;
			this._href = this._constructPath(pathname);
		});
	}

	// TODO: how do we handle this?
	// TODO: use router context
	private _constructPath(sectionPathname: string) {
		return `section/${sectionPathname}/workspace/${this.entityType}`;
	}

	render() {
		return html`<uui-menu-item href="${ifDefined(this._href)}" label=${this.label} ?has-children=${this.hasChildren}
			>${this.#renderIcon()}${this.#renderActions()}<slot></slot
		></uui-menu-item>`;
	}

	#renderIcon() {
		return html` <uui-icon slot="icon" name=${this.iconName}></uui-icon> `;
	}

	#renderActions() {
		return html`<umb-entity-actions-bundle
			slot="actions"
			entity-type=${this.entityType}
			.unique=${null}
			.label=${this.label}>
		</umb-entity-actions-bundle>`;
	}

	static styles = [UUITextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-base': UmbMenuItemBaseElement;
	}
}
