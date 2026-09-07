import type { ExamineField } from './types.js';
import {
  html,
  repeat,
  when,
  css,
  nothing,
  state,
  property,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const MAX_VALUE_LENGTH = 100;

export class UmbSearchExamineDocumentFieldsElement extends UmbLitElement {
  @property({ type: Array })
  fields: Array<ExamineField> = [];

  @state()
  private _filterQuery = '';

  @state()
  private _expandedFields = new Set<string>();

  get #filteredAndSortedFields(): Array<ExamineField> {
    if (!this.fields.length) return [];

    const query = this._filterQuery.toLowerCase();
    return this.fields
      .filter(
        (field) =>
          field.name.toLowerCase().includes(query) ||
          field.values.some((v) => v.toLowerCase().includes(query)),
      )
      .sort((a, b) => a.name.localeCompare(b.name));
  }

  #onFilterInput = (e: InputEvent) => {
    this._filterQuery = (e.target as HTMLInputElement).value;
  };

  #toggleExpanded(fieldKey: string) {
    if (this._expandedFields.has(fieldKey)) {
      this._expandedFields.delete(fieldKey);
    } else {
      this._expandedFields.add(fieldKey);
    }
    this.requestUpdate();
  }

  #renderValue(field: ExamineField, value: string, index: number, showIndex: boolean) {
    const isLong = value.length > MAX_VALUE_LENGTH;
    const fieldKey = `${field.name}-${index}`;
    const isExpanded = this._expandedFields.has(fieldKey);
    const indexPrefix = showIndex
      ? html`<span
          class="value-index"
          title=${this.localize.term('searchExamine_valueIndex', index + 1)}
        >
          [${index}]
        </span>`
      : nothing;

    if (!isLong) {
      return html`
        <div class="value-item">
          ${indexPrefix}
          <span class="value-content">${value}</span>
          <uui-button-copy-text
            class="copy-button"
            .text=${value}
            look="placeholder"
            compact
            label=${this.localize.term('searchExamine_copyValue')}
          ></uui-button-copy-text>
        </div>
      `;
    }

    return html`
      <div class="value-item">
        ${indexPrefix}
        <span class="value-content">
          ${isExpanded ? value : `${value.substring(0, MAX_VALUE_LENGTH)}...`}
          <button class="see-more" @click=${() => this.#toggleExpanded(fieldKey)}>
            ${
              isExpanded
                ? this.localize.term('searchExamine_seeLess')
                : this.localize.term('searchExamine_seeMore')
            }
          </button>
        </span>
        <uui-button-copy-text
          class="copy-button"
          .text=${value}
          look="placeholder"
          compact
          label=${this.localize.term('searchExamine_copyValue')}
        ></uui-button-copy-text>
      </div>
    `;
  }

  #renderField(field: ExamineField) {
    const showIndex = field.values.length > 1;

    return html`
      <tr class="field-row">
        <td class="field-name">
          ${field.name}
          ${
            field.type
              ? html`<uui-icon
                  name="icon-info"
                  title=${this.localize.term('searchExamine_fieldType', field.type)}
                  class="type-icon"
                ></uui-icon>`
              : nothing
          }
        </td>
        <td class="field-value">
          ${field.values.map((value, index) => this.#renderValue(field, value, index, showIndex))}
        </td>
      </tr>
    `;
  }

  override render() {
    return html`
      <div class="filter-bar">
        <uui-input
          type="search"
          placeholder=${this.localize.term('searchExamine_filterPlaceholder')}
          label=${this.localize.term('searchExamine_filterLabel')}
          .value=${this._filterQuery}
          @input=${this.#onFilterInput}
        >
          <uui-icon
            name="icon-search"
            slot="prepend"
            style="padding-left:var(--uui-size-space-2)"
          ></uui-icon>
        </uui-input>
        <span class="field-count">
          ${this.localize.term('searchExamine_fieldCount', this.#filteredAndSortedFields.length)}
        </span>
      </div>
      ${when(
        this.fields.length === 0,
        () => html`<div class="empty-state">${this.localize.term('searchExamine_noFields')}</div>`,
        () =>
          when(
            this.#filteredAndSortedFields.length > 0,
            () => html`
              <uui-box>
                <table class="fields-table">
                  <thead>
                    <tr>
                      <th class="th-name">
                        ${this.localize.term('searchExamine_tableColumnName')}
                      </th>
                      <th class="th-value">
                        ${this.localize.term('searchExamine_tableColumnValue')}
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    ${repeat(
                      this.#filteredAndSortedFields,
                      (field) => field.name,
                      (field) => this.#renderField(field),
                    )}
                  </tbody>
                </table>
              </uui-box>
            `,
            () =>
              html`<div class="empty-state">
                ${this.localize.term('searchExamine_noFieldsMatch')}
              </div>`,
          ),
      )}
    `;
  }

  static override styles = [
    css`
      :host {
        display: block;
        --umb-search-filter-bar-height: 53px;
      }

      .filter-bar {
        display: flex;
        align-items: center;
        gap: var(--uui-size-4);
        padding: var(--uui-size-4);
        border-bottom: 1px solid var(--uui-color-border);
        position: sticky;
        top: 0;
        background: var(--uui-color-background);
        z-index: 1;
      }

      .filter-bar uui-input {
        flex: 1;
      }

      .field-count {
        font-size: var(--uui-font-size-2);
        color: var(--uui-color-text-muted);
        white-space: nowrap;
      }

      .fields-table {
        width: 100%;
        border-collapse: collapse;
        font-size: var(--uui-font-size-3);
      }

      .fields-table th {
        text-align: left;
        padding: var(--uui-size-3) var(--uui-size-4);
        font-weight: 600;
        color: var(--uui-color-text-muted);
        border-bottom: 1px solid var(--uui-color-border);
        position: sticky;
        top: var(--umb-search-filter-bar-height);
        background: var(--uui-color-background);
      }

      .th-name {
        width: 200px;
      }

      .th-value {
        width: auto;
      }

      .field-row {
        border-bottom: 1px solid var(--uui-color-border);
      }

      .field-row:hover {
        background: var(--uui-color-surface-alt);
      }

      .field-row td {
        padding: var(--uui-size-3) var(--uui-size-4);
        vertical-align: top;
      }

      .field-name {
        font-weight: 600;
        font-family: var(--uui-font-family-monospace);
        word-break: break-word;
      }

      .type-icon {
        margin-left: var(--uui-size-2);
        color: var(--uui-color-text-muted);
        font-size: var(--uui-font-size-2);
        cursor: help;
        vertical-align: middle;
      }

      .field-value {
        font-family: var(--uui-font-family-monospace);
        color: var(--uui-color-text);
        line-height: 1.4;
      }

      .value-item {
        display: flex;
        align-items: flex-start;
        gap: var(--uui-size-2);
        padding: var(--uui-size-2);
        border-radius: var(--uui-border-radius);
        margin-bottom: var(--uui-size-2);
        word-break: break-word;
      }

      .value-item:last-child {
        margin-bottom: 0;
      }

      .value-content {
        flex: 1;
        min-width: 0;
      }

      .copy-button {
        opacity: 0;
        transition: opacity 0.15s ease;
        margin-left: auto;
        flex-shrink: 0;
      }

      .value-item:hover .copy-button,
      .value-item:focus-within .copy-button {
        opacity: 1;
      }

      .value-index {
        margin-right: var(--uui-size-2);
        user-select: none;
        opacity: 0.7;
        flex-shrink: 0;
        white-space: nowrap;
      }

      .see-more {
        display: inline;
        background: none;
        border: none;
        color: var(--uui-color-interactive);
        cursor: pointer;
        padding: 0;
        margin-left: var(--uui-size-2);
        font-size: var(--uui-font-size-2);
        text-decoration: underline;
        white-space: nowrap;
      }

      .see-more:hover,
      .see-more:focus-visible {
        color: var(--uui-color-interactive-emphasis);
      }

      .see-more:focus-visible {
        outline: 2px solid var(--uui-color-focus);
        outline-offset: 2px;
        border-radius: var(--uui-border-radius);
      }

      .empty-state {
        padding: var(--uui-size-6);
        text-align: center;
        color: var(--uui-color-text-muted);
      }
    `,
  ];
}

customElements.define('umb-search-examine-document-fields', UmbSearchExamineDocumentFieldsElement);
