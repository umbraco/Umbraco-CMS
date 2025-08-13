import type { UmbSectionSidebarMenuElement } from './section-sidebar-menu.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './section-sidebar-menu.element.js';

export default {
	title: 'Extension Type/Section/Components/Section Sidebar Menu',
	component: 'umb-section-sidebar-menu',
	id: 'umb-section-sidebar-menu',
} as Meta;

export const Docs: StoryFn<UmbSectionSidebarMenuElement> = () =>
	html` <umb-section-sidebar-menu></umb-section-sidebar-menu>`;
