/**
 * Scrolls the main app content container to the top.
 * Falls back to window scrolling if the container is not found.
 */
export const scrollToTop = () => {
  const scrollContainer = document.querySelector('.app-shell__content')

  if (scrollContainer) {
    scrollContainer.scrollTo({ top: 0, behavior: 'smooth' })
    return
  }

  window.scrollTo({ top: 0, behavior: 'smooth' })
}