import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbSectionSidebarContext, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-entity-actions-bundle')
export class UmbEntityActionsBundleElement extends UmbLitElement {
	private _entityType?: string;
	@property({ type: String, attribute: 'entity-type' })
	public get entityType() {
		return this._entityType;
	}
	public set entityType(value: string | undefined) {
		const oldValue = this._entityType;
		if (oldValue === value) return;

		this._entityType = value;
		this.#observeEntityActions();
		this.requestUpdate('entityType', oldValue);
	}

	@property({ type: String })
	unique?: string | null;

	@property({ type: String })
	public label?: string;

	@state()
	private _hasActions = false;

	#sectionSidebarContext?: UmbSectionSidebarContext;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (sectionContext) => {
			this.#sectionSidebarContext = sectionContext;
		});
	}

	#observeEntityActions() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.conditions.entityTypes.includes(this.entityType!)))),
			(actions) => {
				this._hasActions = actions.length > 0;
			},
			'observeEntityAction'
		);
	}

	private _openActions() {
		if (!this.entityType) throw new Error('Entity type is not defined');
		if (this.unique === undefined) throw new Error('Unique is not defined');
		this.#sectionSidebarContext?.toggleContextMenu(this.entityType, this.unique, this.label);
	}

	render() {
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
		'umb-entity-actions-bundle': UmbEntityActionsBundleElement;
	}
}
