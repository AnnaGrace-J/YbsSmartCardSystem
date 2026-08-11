---
name: tailwind-minimal-ui-ux-rules
description: Strict Tailwind CSS UI/UX rules for minimal design, compact layouts, non-blocking alerts, and high-quality action buttons.
---

# UI/UX Minimal Layout Rules (Tailwind CSS Edition)

You are an expert Frontend Engineer and UI/UX Designer. Follow these strict rules for all layout, styling, and interaction patterns in this project using **Tailwind CSS**.

---

## 1. Minimal Layout & Spacing Engine

*   **Compact Grid Scale:** Use concise spacing (`gap-1.5`, `gap-2`, `gap-3`, `gap-4`). Avoid default padding exceeding `p-4` on desktop or `p-3` on mobile for internal card elements.
*   **Density & Screen Real Estate:** 
    *   Target compact, high-density interfaces. 
    *   Avoid giant hero sections, huge whitespace gutters, or oversized card borders.
    *   Keep main view panels constrained with `max-w-4xl` or `max-w-5xl` centered layout unless building a dense multi-column dashboard.
*   **Border-First Depth:**
    *   **Do NOT rely on heavy drop shadows.** Use subtle borders instead: `border border-neutral-200/80 dark:border-neutral-800/80`.
    *   If elevation is required, use ultra-soft shadow: `shadow-xs` or `shadow-sm`.
*   **Subtle Surface Layering:**
    *   Use background color shifts rather than nested card containers.
    *   Base Page: `bg-neutral-50 dark:bg-neutral-950`
    *   Card / Surface: `bg-white dark:bg-neutral-900`
    *   Nested / Muted Area: `bg-neutral-100/60 dark:bg-neutral-800/50`
    *   *Maximum nesting depth:* 1 border card level.

---

## 2. Strictly Prohibit Native Alerts & Heavy Banners

*   **Zero Native Popups:** **NEVER** use `window.alert()`, `window.confirm()`, or `window.prompt()`.
*   **Non-Blocking Toast Notifications:**
    *   Use a lightweight toast provider (e.g., Sonner or React Hot Toast).
    *   Fixed location: `bottom-right` or `bottom-center`.
    *   Toast containers must use minimal utility classes: `text-xs py-2 px-3 rounded-lg shadow-md border border-neutral-200 dark:border-neutral-800`.
    *   Auto-dismiss in `2500ms` - `3000ms`.
*   **Inline Field Feedback:**
    *   Do NOT render big full-width banner alert blocks (e.g., standard bootstrap/daisyUI red alerts).
    *   Render error/warning text directly inline beneath input controls in small typography: `text-[11px] text-red-500 font-medium mt-1`.

---

## 3. Action Button UX & Interactive States

### Visual Hierarchy
Every view or container module must have **at most 1 Primary Action button**.

1.  **Primary Action:**
    *   `bg-neutral-900 text-white hover:bg-neutral-800 dark:bg-neutral-100 dark:text-neutral-900 dark:hover:bg-white`
    *   `px-3 py-1.5 text-xs font-medium rounded-md shadow-xs transition-all duration-150`
2.  **Secondary Action:**
    *   `bg-neutral-100 text-neutral-700 hover:bg-neutral-200/80 dark:bg-neutral-800 dark:text-neutral-300 dark:hover:bg-neutral-700/80`
    *   `border border-neutral-200/60 dark:border-neutral-700/60 px-3 py-1.5 text-xs font-medium rounded-md`
3.  **Ghost / Neutral Action:**
    *   `text-neutral-600 hover:text-neutral-900 hover:bg-neutral-100 dark:text-neutral-400 dark:hover:text-neutral-100 dark:hover:bg-neutral-800/60 px-2.5 py-1.5 text-xs rounded-md`
4.  **Destructive Action:**
    *   `text-red-600 bg-red-50 hover:bg-red-100 dark:text-red-400 dark:bg-red-950/40 dark:hover:bg-red-900/60 px-3 py-1.5 text-xs font-medium rounded-md`

### Interactive Behavior Rules
*   **Click Feel:** Add tactile micro-feedback to all buttons: `active:scale-[0.98] transition-transform duration-100`.
*   **Disabled & Async Loading:**
    *   When an async action is triggered, set `disabled={isLoading}`.
    *   Apply disabled style: `disabled:opacity-50 disabled:cursor-not-allowed disabled:pointer-events-none`.
    *   Replace action icon with a subtle spinning SVG (`animate-spin h-3.5 w-3.5`).
*   **Instant Visual Confirmation:**
    *   For inline actions like "Copy", "Save", or "Apply", switch state briefly (e.g. show checkmark icon + "Copied") for 1500ms before restoring default state.

---

## 4. Typography & Icon Rules

*   **Constrained Font Scale:**
    *   Body default: `text-xs` (`12px` / `13px`) or `text-sm` (`14px`).
    *   Section Headers: `text-base` (`16px`) or `text-lg` (`18px`), `font-semibold`.
    *   Main Page Header: `text-xl` (`20px`), `font-bold`, `tracking-tight`.
    *   Micro Labels / Badges: `text-[10px]` or `text-[11px]`, `uppercase tracking-wider font-semibold text-neutral-500`.
*   **Icon Styling:**
    *   Use 16px to 18px icons (`h-4 w-4` or `h-3.5 w-3.5`).
    *   Maintain consistent stroke weight across all icons (`stroke-[1.5]` or `stroke-[1.75]`).
    *   Always pair standalone icon buttons with tooltips or explicit `aria-label`.

---

## 5. Quick Reference Tailwind Utility Component Code Snippets

### Compact Input Field
```html
<div class="flex flex-col gap-1">
  <label class="text-[11px] font-medium text-neutral-600 dark:text-neutral-400">Project Name</label>
  <input 
    type="text" 
    class="h-8 px-2.5 text-xs rounded-md border border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 focus:outline-hidden focus:ring-1 focus:ring-neutral-900 dark:focus:ring-neutral-100 transition-all"
    placeholder="Enter name..."
  />
  <!-- Inline error message -->
  <span class="text-[11px] text-red-500 font-medium">Name is required</span>
</div>
```

### Minimal Action Button Group
```html
<div class="flex items-center gap-2">
  <button class="h-8 px-3 text-xs font-medium text-neutral-600 dark:text-neutral-400 hover:bg-neutral-100 dark:hover:bg-neutral-800 rounded-md transition-colors active:scale-[0.98]">
    Cancel
  </button>
  <button class="h-8 px-3 text-xs font-medium text-white bg-neutral-900 dark:bg-neutral-100 dark:text-neutral-900 hover:bg-neutral-800 dark:hover:bg-white rounded-md shadow-xs transition-all active:scale-[0.98] flex items-center gap-1.5 disabled:opacity-50">
    <span>Save changes</span>
  </button>
</div>
```
