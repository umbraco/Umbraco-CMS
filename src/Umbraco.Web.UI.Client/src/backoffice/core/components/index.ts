import { manifests as debugManifests } from './debug/manifests';

// TODO: we need to figure out what components should be available for extensions and load them upfront
// TODO: we need to move these files into their respective folders/silos. We then need a way for a silo to globally register a component
import './body-layout/body-layout.element';
import './footer-layout/footer-layout.element';

// TODO: delete these two and change usage to umb-dropdown
import './button-with-dropdown/button-with-dropdown.element';
import './tooltip-menu/tooltip-menu.element';

import './dropdown/dropdown.element';
import './code-block/code-block.element';
import './debug/debug.element';
import './empty-state/empty-state.element';
import './extension-slot/extension-slot.element';
import './table/table.element';

import './entity-action/entity-action-list.element';
import './entity-action/entity-action.element';

import './entity-bulk-action/entity-bulk-action.element';

import './backoffice-frame/backoffice-header.element';
import './backoffice-frame/backoffice-main.element';
import './backoffice-frame/backoffice-modal-container.element';
import './backoffice-frame/backoffice-notification-container.element';

import './collection/collection.element';
import './collection/collection-toolbar.element';
import './collection/collection-selection-actions.element';

import './input-checkbox-list/input-checkbox-list.element';
import './input-color-picker/input-color-picker.element';
import './input-eye-dropper/input-eye-dropper.element';
import './input-multi-url-picker/input-multi-url-picker.element';
import './input-slider/input-slider.element';
import './input-toggle/input-toggle.element';
import './input-upload-field/input-upload-field.element';

import './property-type-based-property/property-type-based-property.element';

import './ref-property-editor-ui/ref-property-editor-ui.element';
import './property-editor-config/property-editor-config.element';

import './section/section-main/section-main.element';
import './section/section-sidebar/section-sidebar.element';
import './section/section.element';

import './tree/tree.element';
import './tree/entity-tree-item/entity-tree-item.element';
import './tree/tree-menu-item/tree-menu-item.element';

import './menu/menu-item-base/menu-item-base.element';

import './variantable-property/variantable-property.element';
import './property-creator/property-creator.element';

import './header-app/header-app-button.element';

import './history/history-list.element';
import './history/history-item.element';

import './workspace/workspace-action/workspace-action.element';
import './workspace/workspace-editor/workspace-editor.element';
import './workspace/workspace-footer/workspace-footer.element';
import './workspace/workspace-action-menu/workspace-action-menu.element';
import './workspace/workspace-variant/workspace-variant.element';
import './workspace-property/workspace-property.element';

import './variant-selector/variant-selector.element';

import './code-editor';

export * from './table';
export * from './tree/tree.element';
export * from './code-editor';

export const manifests = [...debugManifests];
