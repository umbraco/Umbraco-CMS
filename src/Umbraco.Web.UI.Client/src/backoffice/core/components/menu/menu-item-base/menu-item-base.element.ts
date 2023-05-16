import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { map } from 'rxjs';
import {
	UmbSectionSidebarContext,
	UMB_SECTION_SIDEBAR_CONTEXT_TOKEN,
	UmbSectionContext,
	UMB_SECTION_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestEntityAction, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-menu-item-base')
export class UmbMenuItemBaseElement extends UmbLitElement {
	private _entityType?: string;
	@property({ type: String, attribute: 'entity-type' })
	public get entityType() {
		return this._entityType;
	}
	public set entityType(value: string | undefined) {
		this._entityType = value;
		this.#observeEntityActions();
	}

	@property({ type: String, attribute: 'icon-name' })
	public iconName = '';

	@property({ type: String })
	public label = '';

	@property({ type: Boolean, attribute: 'has-children' })
	public hasChildren = false;

	@state()
	private _href?: string;

	@state()
	private _hasActions = false;

	#sectionContext?: UmbSectionContext;
	#sectionSidebarContext?: UmbSectionSidebarContext;
	#actionObserver?: UmbObserverController<Array<ManifestEntityAction>>;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this.#sectionContext = sectionContext;
			this._observeSection();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (sectionContext) => {
			this.#sectionSidebarContext = sectionContext;
		});
	}

	#observeEntityActions() {
		if (this.#actionObserver) this.#actionObserver.destroy();

		this.#actionObserver = this.observe(
			umbExtensionsRegistry
				.extensionsOfType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.conditions.entityTypes.includes(this.entityType!)))),
			(actions) => {
				this._hasActions = actions.length > 0;
			},
			'entityAction'
		);
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

	private _openActions() {
		if (!this.entityType) throw new Error('Entity type is not defined');
		this.#sectionSidebarContext?.toggleContextMenu(this.entityType, undefined, this.label);
	}

	render() {
		return html` <uui-menu-item href="${ifDefined(this._href)}" label=${this.label} ?has-children=${this.hasChildren}
			>${this.#renderIcon()}${this.#renderActions()}<slot></slot
		></uui-menu-item>`;
	}

	#renderIcon() {
		return html` <uui-icon slot="icon" name=${this.iconName}></uui-icon> `;
	}

	#renderActions() {
		return html`
			${this._hasActions
				? html`
						<uui-action-bar slot="actions">
							<uui-button @click=${this._openActions} label="Open actions menu">
								<uui-symbol-more></uui-symbol-more>
							</uui-button>
						</uui-action-bar>
				  `
				: nothing}
		`;
	}

	static styles = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-base': UmbMenuItemBaseElement;
	}
}
