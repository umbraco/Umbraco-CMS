import type { UmbSectionSidebarMenuElement } from './section-sidebar-menu.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './section-sidebar-menu.element.js';

export default {
	title: 'Sections/Shared/Section Sidebar Menu',
	component: 'umb-section-sidebar-menu',
	id: 'umb-section-sidebar-menu',
} as Meta;

export const AAAOverview: StoryFn<UmbSectionSidebarMenuElement> = () =>
	html` <umb-section-sidebar-menu></umb-section-sidebar-menu>`;
AAAOverview.storyName = 'Overview';
