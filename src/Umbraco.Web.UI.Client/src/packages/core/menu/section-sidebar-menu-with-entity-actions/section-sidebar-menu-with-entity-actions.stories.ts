import type { UmbSectionSidebarMenuWithEntityActionsElement } from './section-sidebar-menu-with-entity-actions.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './section-sidebar-menu-with-entity-actions.element.js';

export default {
	title: 'Extension Type/Section/Components/Section Sidebar Menu With Entity Actions',
	component: 'umb-section-sidebar-menu-with-entity-actions',
	id: 'umb-section-sidebar-menu-with-entity-actions',
} as Meta;

export const Docs: StoryFn<UmbSectionSidebarMenuWithEntityActionsElement> = () =>
	html` <umb-section-sidebar-menu-with-entity-actions></umb-section-sidebar-menu-with-entity-actions>`;
