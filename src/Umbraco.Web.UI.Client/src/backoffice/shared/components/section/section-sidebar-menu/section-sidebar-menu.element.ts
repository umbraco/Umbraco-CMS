import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section.context';
import { ManifestSidebarMenuItem } from '@umbraco-cms/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/element';

import './sidebar-menu-item.element.ts';

@customElement('umb-section-sidebar-menu')
export class UmbSectionSidebarMenuElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@state()
	private _currentSectionAlias?: string;

	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (instance) => {
			this._sectionContext = instance;
			this._observeCurrentSection();
		});
	}

	private _observeCurrentSection() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext.alias, (alias) => {
			this._currentSectionAlias = alias;
		});
	}

	render() {
		return html` <umb-extension-slot
			type="sidebarMenuItem"
			.filter=${(items: ManifestSidebarMenuItem) => items.meta.sections.includes(this._currentSectionAlias || '')}
			default-element="umb-sidebar-menu-item"></umb-extension-slot>`;
	}
}

export default UmbSectionSidebarMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-menu': UmbSectionSidebarMenuElement;
	}
}
